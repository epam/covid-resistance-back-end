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

using Epam.CovidResistance.Services.User.API.Extensions;
using Epam.CovidResistance.Services.User.API.Interfaces;
using Epam.CovidResistance.Services.User.API.Models;
using Epam.CovidResistance.Services.User.Application.Common.Interfaces;
using Epam.CovidResistance.Services.User.Application.Common.Models;
using Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions;
using Epam.CovidResistance.Shared.Domain.Model.Errors;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.User.API.Controllers
{
    /// <summary>
    /// Represents the controller for account endpoints.
    /// </summary>
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme, Roles = "User"),
     Route("api/v1/[controller]"), ApiController]
    public class AccountController : BaseApiController
    {
        private readonly IIdentityService identityService;
        private readonly IUserStateService userStateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"></see> class.
        /// </summary>
        public AccountController(IIdentityService identityService, IUserStateService userStateService)
        {
            this.identityService = identityService;
            this.userStateService = userStateService;
        }

        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="registerForm">Parameters for user registration.</param>
        /// <param name="cancellationToken">The cancellation token for request.</param>
        /// <returns>User access and refresh tokens.</returns>
        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerForm, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Result.Failure(ModelState.ToInnerErrors()));
            }

            (Result createResult, var userToken) = await identityService.CreateUserAsync(registerForm.UserToken,
                                                       registerForm.Username,
                                                       registerForm.Password);

            if (!createResult.Succeeded)
            {
                return createResult.Status == ResultStatus.Validation
                           ? BadRequest(createResult)
                           : InternalServerError(createResult);
            }

            try
            {
                userStateService.RegisterUser(registerForm.UserToken, registerForm.Username);
            }
            catch (Exception ex)
            {
                Result deleteResult = await identityService.DeleteUserAsync(userToken);

                return InternalServerError(
                    Result.Failure(deleteResult.Errors.Append(new InnerError(ErrorTarget.UserStateFailure, ex.Message))));
            }

            (Result tokenResult, Token tokens) = await identityService.PostRegisterLoginAsync(registerForm.Username,
                                                     registerForm.Password,
                                                     cancellationToken);

            return tokenResult.Succeeded
                       ? Ok(tokens)
                       : InternalServerError(tokenResult);
        }

        /// <summary>
        /// Nominates the user as medical.
        /// </summary>
        /// <param name="nominationRequest">Parameters for nominating the user as medical.</param>
        [HttpPost("nominateAsMedical")]
        public async Task<IActionResult> AddToRole([FromBody] MedicalNominationRequest nominationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Result.Failure(ModelState.ToInnerErrors()));
            }

            Result changeResult =
                await identityService.AddToMedicalRoleAsync(User.GetToken(), nominationRequest.HealthSecurityId);

            if (changeResult.Status == ResultStatus.Validation)
            {
                return BadRequest(changeResult);
            }

            return changeResult.Succeeded
                       ? Ok()
                       : InternalServerError(changeResult);
        }
    }
}