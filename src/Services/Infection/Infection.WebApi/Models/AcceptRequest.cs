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

namespace Epam.CovidResistance.Services.Infection.WebApi.Models
{
    /// <summary>
    /// Represent a request to accept the changing of user status.
    /// </summary>
    public class AcceptRequest
    {
        /// <summary>Gets or sets the one time medical code.</summary>
        /// <value>The medical code.</value>
        [Required]
        public string MedicalCode { get; set; }

        /// <summary>Gets or sets the token meetings.</summary>
        /// <value>The meetings.</value>
        public IEnumerable<Meeting> Meetings { get; set; }

        /// <summary>Initializes a new instance of the <see cref="AcceptRequest" /> class.</summary>
        public AcceptRequest()
        {
            Meetings = new List<Meeting>();
        }
    }
}