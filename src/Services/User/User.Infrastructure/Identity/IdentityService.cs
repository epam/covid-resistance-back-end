// =========================================================================
// Copyright 2020 EPAM Systems, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =========================================================================

using Epam.CovidResistance.Services.User.Application.Common.Interfaces;
using Epam.CovidResistance.Services.User.Application.Common.Models;
using Epam.CovidResistance.Services.User.Infrastructure.Extensions;
using Epam.CovidResistance.Services.User.Infrastructure.Models;
using Epam.CovidResistance.Shared.Domain.Model.Errors;
using Epam.CovidResistance.Shared.IdentityDbContext.Identity;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.User.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly ClientIdentityOptions clientIdentity;
        private readonly HttpClient httpClient;
        private readonly ILogger<IdentityService> logger;
        private readonly IMedicalRegistrationRepository medicalRegistrationRepository;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityService"></see> class.
        /// </summary>
        public IdentityService(
            AspNetUserManager<ApplicationUser> userManager,
            IHttpClientFactory clientFactory,
            IMedicalRegistrationRepository medicalRegistrationRepository,
            IOptions<ClientIdentityOptions> clientIdentityOptions,
            ILogger<IdentityService> logger)
        {
            this.userManager = userManager;
            this.medicalRegistrationRepository = medicalRegistrationRepository;
            this.logger = logger;
            httpClient = clientFactory.CreateClient();
            clientIdentity = clientIdentityOptions.Value;
        }

        public async Task<(Result Result, string UserId)> CreateUserAsync(string userToken,
            string userName,
            string password)
        {
            logger.LogInformation("Start creating {userName} user with token {userToken}.", userName, userToken);

            //The identifier is not verified on the ASP.NET Identity side
            (Result findResult, ApplicationUser user) = await FindUserByTokenAsync(userToken);

            if (findResult.Succeeded)
            {
                Result result = Result.ValidationError(new InnerError(ErrorTarget.DuplicateUserToken, "This user token is already taken."));

                logger.LogError("Error creating user with token {userToken}. Errors: {@message}", userToken, result.Errors);

                return (result, user.Id);
            }

            user = new ApplicationUser(userToken)
            {
                UserName = userName
            };

            IdentityResult identityResult = await userManager.CreateAsync(user, password);

            if (!identityResult.Succeeded)
            {
                Result result = identityResult.ToApplicationResult();

                logger.LogError("Error creating user with token {userToken}. Errors: {@message}", userToken, result.Errors);

                return (result, user.Id);
            }

            logger.LogInformation("Successfully created {userName} user with token {userToken}.", userName, userToken);

            var userRoleName = Roles.User.ToString("G");

            identityResult = await userManager.AddToRoleAsync(user, userRoleName);

            if (!identityResult.Succeeded)
            {
                Result result = identityResult.ToApplicationResult();

                logger.LogError("Error on assignment role {userRoleName} for user with token {userToken}. Errors: {@message}",
                    userRoleName,
                    userToken,
                    result.Errors);

                return (result, user.Id);
            }

            logger.LogInformation("Successfully assigned role {userRoleName} for {userName} user with token {userToken}.",
                userRoleName,
                userName,
                userToken);

            return (Result.Success(), user.Id);
        }

        public async Task<Result> DeleteUserAsync(string userToken)
        {
            logger.LogInformation("Start deleting of user with token {userToken}.", userToken);

            (Result result, ApplicationUser user) = await FindUserByTokenAsync(userToken);

            if (!result.Succeeded)
            {
                logger.LogError("Error deleting user with token {userToken}. Errors: {@message}", userToken, result.Errors);

                return result;
            }

            IdentityResult deleteResult = await userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                result = deleteResult.ToApplicationResult();

                logger.LogError("Error deleting user with token {userToken}. Errors: {@message}", userToken, result.Errors);

                return result;
            }

            logger.LogInformation("Successfully deleted {userName} user with token {userToken}.", user.UserName, userToken);

            return Result.Success();
        }

        public async Task<Result> AddToMedicalRoleAsync(string userToken, string healthSecurityId)
        {
            var medicalRoleName = Roles.Medical.ToString("G");

            logger.LogInformation("Start assignment of user with token {userToken} to {medicalRole} role.",
                userToken,
                medicalRoleName);

            (Result searchResult, ApplicationUser user) = await FindUserByTokenAsync(userToken);

            if (!searchResult.Succeeded)
            {
                logger.LogError("Error on assignment user with token {userToken}. Errors: {@message}",
                    userToken,
                    searchResult.Errors);

                return searchResult;
            }

            if (await userManager.IsInRoleAsync(user, medicalRoleName))
            {
                logger.LogInformation("User with token {userToken} already assignment to {medicalRole} role.",
                    userToken,
                    medicalRoleName);

                return Result.Success();
            }

            if (!medicalRegistrationRepository.TryRegistration(healthSecurityId))
            {
                Result result = Result.Failure(new InnerError(ErrorTarget.InvalidHealthSecurityId,
                    "Health security id is already taken or expired."));

                logger.LogWarning("Unsuccessful result on validation of the security Id {healthSecurityId}. Errors: {@message}",
                    healthSecurityId,
                    result.Errors);

                return result;
            }

            IdentityResult roleAssignmentResult = await userManager.AddToRoleAsync(user, medicalRoleName);

            if (!roleAssignmentResult.Succeeded)
            {
                medicalRegistrationRepository.RollBackRegistration(healthSecurityId);

                Result result = roleAssignmentResult.ToApplicationResult();

                logger.LogError("Error on assignment user with token {userToken} to {medicalRole} role. Errors: {@message}",
                    userToken,
                    medicalRoleName,
                    result.Errors);

                return result;
            }

            logger.LogInformation("Successfully assigned {userName} user with token {userToken} to {medicalRole} role..",
                user.UserName,
                userToken,
                medicalRoleName);

            return Result.Success();
        }

        public async Task<(Result Result, string[] UserRoles)> GetUserRolesAsync(string userToken)
        {
            logger.LogInformation("Start retrieving the role for user with token {userToken}.", userToken);

            (Result searchResult, ApplicationUser user) = await FindUserByTokenAsync(userToken);

            if (!searchResult.Succeeded)
            {
                logger.LogError("Error retrieving roles for user with token {userToken}. Errors: {@message}",
                    userToken,
                    searchResult.Errors);

                return (searchResult, Array.Empty<string>());
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            logger.LogInformation("Successfully retrieved roles for {userName} user with token {userToken}.",
                user.UserName,
                userToken);

            return (Result.Success(), roles.ToArray());
        }

        public async Task<(Result Result, Token Tokens)> PostRegisterLoginAsync(string userName,
            string password,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting login in {userName}", userName);

            DiscoveryDocumentResponse discoveryDocument = await httpClient.GetDiscoveryDocumentAsync(
                                                              clientIdentity.Authority,
                                                              cancellationToken);

            TokenResponse tokenResponse =
                await RequestTokenAsync(userName, password, discoveryDocument.TokenEndpoint, cancellationToken);

            if (tokenResponse.IsError)
            {
                Result result = Result.Failure(new InnerError(ErrorTarget.Token, tokenResponse.Error));

                logger.LogInformation("Error on login in {userName}. Errors: {@message}", userName, result.Errors);

                return (result, (Token)null);
            }

            logger.LogInformation("Successfully login in {userName}", userName);

            return (Result.Success(),
                       new Token(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.ExpiresIn));
        }

        private async Task<(Result Result, ApplicationUser User)> FindUserByTokenAsync(string userToken)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userToken);

            return user != null
                       ? (Result.Success(), user)
                       : (Result.ValidationError(new InnerError(ErrorTarget.UserNotFound, "Unable to find the user.")), null);
        }

        private async Task<TokenResponse> RequestTokenAsync(string user,
            string password,
            string tokenEndpoint,
            CancellationToken cancellationToken)
            => await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
                   {
                       Address = tokenEndpoint,

                       ClientId = clientIdentity.ClientId,
                       Scope = clientIdentity.Scope,

                       UserName = user,
                       Password = password
                   },
                   cancellationToken);
    }
}