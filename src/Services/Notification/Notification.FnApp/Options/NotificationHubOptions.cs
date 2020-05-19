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

namespace Epam.CovidResistance.Services.Notification.FnApp.Options
{
    /// <summary>
    /// Represents connection options to notification hub.
    /// </summary>
    public class NotificationHubOptions
    {
        /// <summary>Gets or sets the connection string.</summary>
        /// <value>The connection string.</value>
        public string Connection { get; set; }

        /// <summary>Gets or sets the notification hub name.</summary>
        /// <value>The hub name.</value>
        public string Name { get; set; }
    }
}