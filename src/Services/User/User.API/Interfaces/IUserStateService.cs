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

using Epam.CovidResistance.Services.User.API.Models;
using Epam.CovidResistance.Shared.Domain.Model;

namespace Epam.CovidResistance.Services.User.API.Interfaces
{
    /// <summary>
    /// The interface for user state management.
    /// </summary>
    public interface IUserStateService
    {
        /// <summary>
        /// Gets the user information by user token. 
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns>The user information.</returns>
        UserInfo GetUserInformation(string userToken);

        /// <summary>
        /// Sets push notification information for the specified user token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <param name="pushNotification">Push notification parameters.</param>
        void SetupNotification(string userToken, PushNotification pushNotification);

        /// <summary>
        /// Creates user information.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <param name="username">The username.</param>
        void RegisterUser(string userToken, string username);
    }
}