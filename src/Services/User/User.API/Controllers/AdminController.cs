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

using Epam.CovidResistance.Services.User.API.Interfaces;
using Epam.CovidResistance.Services.User.API.Models;
using Epam.CovidResistance.Services.User.Application.Common.Models;
using Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Epam.CovidResistance.Services.User.API.Controllers
{
    /// <summary>
    /// Represents the controller for admin endpoints.
    /// </summary>
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme, Roles = "Admin"),
     Route("api/v1/[controller]"), ApiController]
    public class AdminController : BaseApiController
    {
        private readonly IHealthSecurityService healthSecurityService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"></see> class.
        /// </summary>
        public AdminController(IHealthSecurityService healthSecurityService)
        {
            this.healthSecurityService = healthSecurityService;
        }

        /// <summary>
        /// Generates health security ids.
        /// </summary>
        /// <param name="request">Parameters for generating health security ids.</param>
        /// <returns>Health security ids.</returns>
        [HttpPost("generatehealthsecurityids")]
        public IActionResult GenerateHealthSecurityIds([FromBody] HealthSecurityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Result.Failure(ModelState.ToInnerErrors()));
            }

            (Result result, string[] securityIds) = GetExecutionResult(() => healthSecurityService.GenerateSecurityIds(request.NumberOfCodes.Value, request.CodeLength.Value, request.Comment));

            return result.Succeeded
                       ? Ok(new HealthSecurityResponse { HealthSecurityIds = securityIds })
                       : InternalServerError(result);
        }
    }
}