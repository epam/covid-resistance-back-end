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
using System.ComponentModel.DataAnnotations;

namespace Epam.CovidResistance.Services.Notification.FnApp.Entities
{
    /// <summary>
    /// The push notification.
    /// </summary>
    public class PushNotification
    {
        /// <summary>Gets or sets the PNS handle.</summary>
        /// <value>The device handle.</value>
        [Required]
        public string Handle { get; set; }

        /// <summary>Gets or sets the notification service platform.</summary>
        /// <value>The platform.</value>
        [Required]
        public string Platform { get; set; }

        /// <summary>Gets or sets the message.</summary>
        /// <value>The message.</value>
        [Required]
        public string Message { get; set; }

        /// <summary>Returns true if ... is valid.</summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public bool IsValid()
        {
            var validationResults = new List<ValidationResult>();
            var result = Validator.TryValidateObject(this, new ValidationContext(this, null, null), validationResults, true);

            return result;
        }
    }
}