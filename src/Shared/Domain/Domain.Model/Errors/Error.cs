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

using System.Collections.Generic;
using System.Linq;

namespace Epam.CovidResistance.Shared.Domain.Model.Errors
{
    /// <summary>
    /// Represents error details.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Error"></see> class with empty inner errors.
        /// </summary>
        public Error(string errorCode)
        {
            ErrorCode = errorCode;
            Errors = new List<InnerError>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"></see> class.
        /// </summary>
        public Error(string errorCode, IEnumerable<InnerError> innerErrors)
        {
            ErrorCode = errorCode;
            Errors = innerErrors.ToList();
        }

        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets inner errors.
        /// </summary>
        public List<InnerError> Errors { get; set; }
    }

    /// <summary>
    /// Represents inner error.
    /// </summary>
    public readonly struct InnerError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InnerError"></see> struct.
        /// </summary>
        public InnerError(string errorTarget, string message)
        {
            ErrorTarget = errorTarget;
            Message = message;
        }

        /// <summary>
        /// Gets or sets machine-friendly error target which is used in error handling.
        /// </summary>
        public string ErrorTarget { get; }

        /// <summary>
        /// Gets or sets human-readable error message.
        /// </summary>
        public string Message { get; }
    }
}
