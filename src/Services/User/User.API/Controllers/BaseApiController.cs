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

using Epam.CovidResistance.Services.User.Application.Common.Models;
using Epam.CovidResistance.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Epam.CovidResistance.Services.User.API.Controllers
{
    /// <summary>
    /// Represents the base controller.
    /// </summary>
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Gets the result of execution of the function.
        /// </summary>
        /// <typeparam name="T">The type of returned value from the function.</typeparam>
        /// <param name="function">The function for execution.</param>
        /// <returns>The result and value tuple of execution of the function.</returns>
        public (Result Result, T Value) GetExecutionResult<T>(Func<T> function)
        {
            try
            {
                T result = function();
                return (Result.Success(), result);
            }
            catch (Exception ex)
            {
                return (Result.FromException(ex), default);
            }
        }

        /// <summary>
        /// Gets the result of execution of the action.
        /// </summary>
        /// <param name="action">The action for execution.</param>
        /// <returns>The result of execution of the action.</returns>
        public Result GetExecutionResult(Action action)
        {
            try
            {
                action();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// Creates an <see cref="BadRequestObjectResult"/> with the details about execution result errors.
        /// </summary>
        /// <param name="result">The execution result.</param>
        /// <returns>The <see cref="IActionResult"/> with status code 400.</returns>
        public IActionResult BadRequest(Result result)
            => BadRequest(new Error(StatusCodes.Status400BadRequest.ToString(), result.Errors));

        /// <summary>
        /// Creates an <see cref="ObjectResult"/> that produces a <see cref="StatusCodes.Status500InternalServerError"/>
        /// response with the details of execution result.
        /// </summary>
        /// <param name="result">The execution result.</param>
        /// <returns>The <see cref="IActionResult"/> with status code 500.</returns>
        public IActionResult InternalServerError(Result result)
            => StatusCode(StatusCodes.Status500InternalServerError, result);

        /// <summary>
        /// Creates an <see cref="ObjectResult"/> that produces a response with specified status code
        /// and the details about execution result errors.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="result">The execution result.</param>
        /// <returns>The <see cref="IActionResult"/> with the specified status code.</returns>
        public IActionResult StatusCode(int statusCode, Result result)
            => StatusCode(statusCode, new Error(statusCode.ToString(), result.Errors));
    }
}