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

using System.ComponentModel.DataAnnotations;

namespace Epam.CovidResistance.Services.User.API.Models
{
    /// <summary>
    /// Represents a request for setting up push notification information.
    /// </summary>
    public class PushNotification
    {
        /// <summary>
        /// Gets or sets push notification token.
        /// </summary>
        [Required]
        public string PushNotificationToken { get; set; }

        /// <summary>
        /// Gets or sets push notification target.
        /// </summary>
        [Required]
        public string PushNotificationTarget { get; set; }
    }
}