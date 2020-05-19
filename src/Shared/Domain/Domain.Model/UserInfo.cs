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
    /// Describes user information.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"></see> class.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="statusId">The user status id.</param>
        /// <param name="statusChangedOn">The user status date-time.</param>
        public UserInfo(string userToken, string userName, int statusId, DateTime statusChangedOn)
        {
            UserToken = userToken;
            UserName = userName;
            StatusId = statusId;
            StatusChangedOn = statusChangedOn;
        }

        /// <summary>
        /// Gets or sets the user token.
        /// </summary>
        public string UserToken { get; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Gets or sets the user status id.
        /// </summary>
        public int StatusId { get; }

        /// <summary>
        /// Gets or sets the user status date-time.
        /// </summary>
        public DateTime StatusChangedOn { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified object.</param>
        public override bool Equals(object obj)
            => obj is UserInfo info &&
               UserToken == info.UserToken &&
               UserName == info.UserName &&
               StatusId == info.StatusId &&
               StatusChangedOn == info.StatusChangedOn;

        /// <summary>
        /// Gets hash code.
        /// </summary>
        public override int GetHashCode()
            => HashCode.Combine(UserToken, UserName, StatusId, StatusChangedOn);
    }
}
