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
using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.User.API.Controllers
{
    /// <summary>
    /// Represents the controller for user endpoints.
    /// </summary>
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme, Roles = "User"),
     Route("api/v1/[controller]"), ApiController]
    public class UserController : BaseApiController
    {
        private readonly IIdentityService identityService;
        private readonly IOptions<Metadata> options;
        private readonly IUserStateService userStateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"></see> class.
        /// </summary>
        public UserController(IUserStateService userStateService,
            IIdentityService identityService,
            IOptions<Metadata> options)
        {
            this.userStateService = userStateService;
            this.identityService = identityService;
            this.options = options;
        }

        /// <summary>
        /// Gets the user profile and metadata which the Mobile application requires for successful running.
        /// </summary>
        /// <returns>User profile and application metadata.</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userToken = User.GetToken();

            (Result stateResult, UserInfo userInfo) = GetExecutionResult(() => userStateService.GetUserInformation(userToken));

            if (!stateResult.Succeeded)
            {
                return InternalServerError(stateResult);
            }

            (Result rolesResult, string[] userRoles) = await identityService.GetUserRolesAsync(userToken);

            if (!rolesResult.Succeeded)
            {
                return InternalServerError(rolesResult);
            }

            ProfileResponse response = BuildProfileResponse(userInfo, userRoles);

            return Ok(response);
        }

        private ProfileResponse BuildProfileResponse(UserInfo userInfo, IEnumerable<string> userRoles)
        {
            var userProfile = new UserProfile
            {
                UserName = userInfo.UserName,
                StatusId = userInfo.StatusId,
                StatusChangedOn = userInfo.StatusChangedOn,
                Roles = userRoles
            };

            var response = new ProfileResponse
            {
                UserProfile = userProfile,
                Metadata = options.Value
            };

            return response;
        }
    }
}