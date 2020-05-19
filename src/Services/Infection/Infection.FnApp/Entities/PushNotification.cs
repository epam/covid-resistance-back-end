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

namespace Epam.CovidResistance.Services.Infection.FnApp.Entities
{
    /// <summary>
    /// Represents a request for sending a push notification.
    /// </summary>
    public class PushNotification
    {
        /// <summary>Gets or sets the device handle.</summary>
        /// <value>The handle.</value>
        public string Handle { get; set; }

        /// <summary>Gets or sets the notification platform.</summary>
        /// <value>The platform.</value>
        public string Platform { get; set; }

        /// <summary>Gets or sets the message.</summary>
        /// <value>The message.</value>
        public string Message { get; set; }
    }
}
