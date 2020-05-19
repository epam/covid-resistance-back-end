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

namespace Epam.CovidResistance.Shared.Domain.Model
{
    /// <summary>
    /// Describes meeting with the contact.
    /// </summary>
    public readonly struct MeetingContact
    {
        /// <summary>
        /// Gets or sets meeting time with the contact.
        /// </summary>
        public DateTime MeetingTime { get; }

        /// <summary>
        /// Gets or sets user token of the contact.
        /// </summary>
        public string UserToken { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingContact"></see> struct.
        /// </summary>
        public MeetingContact(DateTime meetingTime, string userToken)
        {
            MeetingTime = meetingTime;
            UserToken = userToken;
        }
    }
}
