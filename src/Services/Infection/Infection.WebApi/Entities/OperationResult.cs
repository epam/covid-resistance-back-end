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

using Microsoft.AspNetCore.Http;

namespace Epam.CovidResistance.Services.Infection.WebApi.Entities
{
    /// <summary>
    /// Represents an operation result.
    /// </summary>
    public class OperationResult
    {
        /// <summary>Gets a value indicating whether this <see cref="OperationResult" /> is succeeded.</summary>
        /// <value>
        ///   <c>true</c> if succeeded; otherwise, <c>false</c>.</value>
        public bool Succeeded => Status == StatusCodes.Status200OK;

        /// <summary>Gets or sets the status.</summary>
        /// <value>The Http code.</value>
        public int Status { get; set; }

        /// <summary>Gets or sets the error message.</summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>Gets or sets the error target.</summary>
        /// <value>The error target.</value>
        public string ErrorTarget { get; set; }

        /// <summary>Initializes a new instance of the <see cref="OperationResult" /> class.</summary>
        /// <param name="status">The status.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorTarget"></param>
        private OperationResult(int status, string errorMessage = null, string errorTarget = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
            ErrorTarget = errorTarget;
        }

        /// <summary>Creates the instance.</summary>
        /// <param name="status">The status.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorTarget">The error target.</param>
        /// <returns></returns>
        public static OperationResult CreateInstance(int status, string errorMessage = null, string errorTarget = null)
            => new OperationResult(status, errorMessage, errorTarget);

    }
}
