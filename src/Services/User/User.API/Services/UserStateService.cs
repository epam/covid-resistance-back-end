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

using Epam.CovidResistance.Services.User.API.Interfaces;
using Epam.CovidResistance.Services.User.API.Models;
using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Options;
using System;

namespace Epam.CovidResistance.Services.User.API.Services
{
    /// <summary>
    /// The service for user state management.
    /// </summary>
    public class UserStateService : IUserStateService
    {
        private readonly IOptions<Metadata> metadata;
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStateService"/> class.
        /// </summary>
        public UserStateService(IUserRepository userRepository, IOptions<Metadata> metadata)
        {
            this.userRepository = userRepository;
            this.metadata = metadata;
        }

        /// <summary>
        /// Gets the user information by user token. 
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns>The user information.</returns>
        public UserInfo GetUserInformation(string userToken)
            => userRepository.GetUserInfo(userToken);

        /// <summary>
        /// Sets push notification information for the specified user token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <param name="pushNotification">Push notification parameters.</param>
        public void SetupNotification(string userToken, PushNotification pushNotification)
        {
            var userStatePushNotification = new UserStatePushNotification
            {
                UserToken = userToken,
                NotificationToken = pushNotification.PushNotificationToken.Replace(" ", string.Empty).Trim(),
                NotificationTarget = pushNotification.PushNotificationTarget
            };

            userRepository.SetPushNotification(userStatePushNotification);
        }

        /// <summary>
        /// Creates user information.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <param name="username">The username.</param>
        public void RegisterUser(string userToken, string username)
        {
            var userInfo = new UserInfo(userToken, username, metadata.Value.Statuses.Default, DateTime.UtcNow);
            userRepository.RegisterUser(userInfo);
        }
    }
}