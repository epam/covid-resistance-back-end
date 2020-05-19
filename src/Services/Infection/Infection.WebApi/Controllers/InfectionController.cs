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

using Epam.CovidResistance.Services.Infection.WebApi.Entities;
using Epam.CovidResistance.Services.Infection.WebApi.Interfaces;
using Epam.CovidResistance.Services.Infection.WebApi.Models;
using Epam.CovidResistance.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.WebApi.Controllers
{
    /// <summary>
    /// Provides functionality to Infection Service
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InfectionController : ControllerBase
    {
        private readonly IInfectionService infectionService;
        private readonly ILogger<InfectionController> logger;

        /// <summary>Initializes a new instance of the <see cref="InfectionController" /> class.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="infectionService">The infection service.</param>
        public InfectionController(
            ILogger<InfectionController> logger,
            IInfectionService infectionService)
        {
            this.logger = logger;
            this.infectionService = infectionService;
        }

        /// <summary>Initiates a status change request.</summary>
        /// <param name="changeRequest">The change request.</param>
        /// <returns></returns>
        /// <remarks>Called when a doctor initiates a status change request.</remarks>
        [Authorize(Roles = "Medical")]
        [HttpPost("status/init")]
        [ProducesResponseType(typeof(ChangeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        public IActionResult InitStatusChange([FromBody] ChangeRequest changeRequest)
        {
            var userToken = User.FindFirst("sub")?.Value;

            try
            {
                OperationResult operationResult = infectionService.InitStatusChange(userToken, changeRequest, out ChangeResponse initResponse);
                if (operationResult.Succeeded)
                {
                    logger.LogInformation("Change request is saved");

                    return new OkObjectResult(initResponse);
                }

                logger.LogWarning("Status change request is rejected.");

                return InternalServerError(new InnerError(
                        operationResult.ErrorTarget ?? ErrorTarget.MedicalCodeNotGenerated,
                        operationResult.ErrorMessage));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception: {e.Message}");
                return InternalServerError(new InnerError(ErrorTarget.StatusChangeRequestRejected, "Unhandled exception."));
            }
        }

        /// <summary>Accepts the status change request.</summary>
        /// <param name="acceptRequest">The accept request.</param>
        /// <returns></returns>
        /// <remarks>Called when the user accepts the status change request.</remarks>
        [Authorize(Roles = "User")]
        [HttpPost("status/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptStatusChange([FromBody] AcceptRequest acceptRequest)
        {
            try
            {
                var userToken = User.FindFirst("sub")?.Value;

                OperationResult operationResult = await infectionService.AcceptStatusAsync(userToken, acceptRequest);

                switch (operationResult.Status)
                {
                    case StatusCodes.Status200OK:
                        logger.LogInformation("Change request is accepted");
                        return Ok();
                    case StatusCodes.Status404NotFound:
                        logger.LogWarning(operationResult.ErrorMessage);
                        return ActionError(operationResult.Status, new InnerError(ErrorTarget.AcceptStatusCodeNotFound, operationResult.ErrorMessage));
                    case StatusCodes.Status406NotAcceptable:
                        logger.LogWarning(operationResult.ErrorMessage);
                        return ActionError(operationResult.Status, new InnerError(ErrorTarget.AcceptStatusCodeExpired, operationResult.ErrorMessage));
                }
                logger.LogError(operationResult.ErrorMessage);
                return InternalServerError(new InnerError(ErrorTarget.AcceptStatusFailed, "Unhandled exception."));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception: {e.Message}");
                return InternalServerError(new InnerError(ErrorTarget.AcceptStatusRejected, "Unhandled exception."));
            }
        }


        /// <summary>Actions the error.</summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private IActionResult ActionError(int statusCode, InnerError error)
        {
            var errorResponse = new Error(statusCode.ToString());
            errorResponse.Errors.Add(error);

            return StatusCode(statusCode, errorResponse);
        }

        /// <summary>The internal server error.</summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private IActionResult InternalServerError(InnerError error)
            => ActionError(StatusCodes.Status500InternalServerError, error);
    }
}