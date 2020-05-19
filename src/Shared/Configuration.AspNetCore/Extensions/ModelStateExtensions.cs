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

using Epam.CovidResistance.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions
{
    /// <summary>
    /// The model state extensions.
    /// </summary>
    public static class ModelStateExtensions
    {
        /// <summary>
        /// The to inner errors.
        /// </summary>
        /// <param name="stateDictionary">
        /// The state dictionary.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{InnerError}"/>.
        /// </returns>
        public static IEnumerable<InnerError> ToInnerErrors(this ModelStateDictionary stateDictionary)
            => stateDictionary.Keys.SelectMany(
                target => stateDictionary[target].Errors.Select(error => new InnerError(target, error.ErrorMessage)));

        /// <summary>
        /// The invalid model state response factory.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        public static IActionResult InvalidModelStateResponseFactory(ActionContext context)
            => new BadRequestObjectResult(
                new Error(StatusCodes.Status400BadRequest.ToString(), context.ModelState.ToInnerErrors()));
    }
}