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

using System;
using System.ComponentModel.DataAnnotations;

namespace Epam.CovidResistance.Services.Infection.WebApi.Models
{
    /// <summary>
    /// Represent request to change user status.
    /// </summary>
    public class ChangeRequest
    {
        /// <summary>Gets or sets the token status id.</summary>
        /// <value>The status id.</value>
        [Required]
        public int? StatusId { get; set; }

        /// <summary>Gets or sets the date of status changed on.</summary>
        /// <value>The status changed on.</value>
        [Required]
        public DateTime? StatusChangedOn { get; set; }

        /// <summary>Gets or sets the comment.</summary>
        /// <value>The comment.</value>
        public string Comment { get; set; }
    }
}