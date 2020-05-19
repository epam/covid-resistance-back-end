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

using Epam.CovidResistance.Services.Infection.FnApp.Entities;
using Epam.CovidResistance.Services.Infection.FnApp.Helpers;
using Epam.CovidResistance.Services.Infection.FnApp.Interfaces;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.FnApp.Services
{
    /// <summary>
    /// The exposed contact service.
    /// </summary>
    public class ExposedContactService : IExposedContactService
    {
        private readonly IUserRepository userRepository;
        private readonly IOptions<Metadata> options;
        private readonly IOptions<Backend> backendOptions;
        private readonly ILogger<ExposedContactService> logger;

        /// <summary>Gets or sets the message sender.</summary>
        /// <value>The message sender.</value>
        public MessageSender MessageSender { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ExposedContactService" /> class.</summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="options">The options.</param>
        /// <param name="backendOptions">The backend options.</param>
        /// <param name="logger">The logger.</param>
        public ExposedContactService(
            IUserRepository userRepository,
            IOptions<Metadata> options,
            IOptions<Backend> backendOptions,
            ILogger<ExposedContactService> logger)
        {
            this.userRepository = userRepository;
            this.options = options;
            this.backendOptions = backendOptions;
            this.logger = logger;
        }

        /// <summary>Processes the contact.</summary>
        /// <param name="exposedContact">The exposed contact.</param>
        /// <exception cref="ApplicationException">User token not found
        /// or
        /// Invalid configuration: Statuses.OnExposure not found
        /// or
        /// User status not found</exception>
        public async Task ProcessContact(ExposedContact exposedContact)
        {
            // check user status
            UserState userState = userRepository.GetUserState(exposedContact.UserToken);

            if (userState == null)
            {
                throw new ApplicationException("User token not found");
            }

            Statuses statuses = options.Value.Statuses;
            var exposureStatusId = statuses.OnExposure;
            Status exposureStatus = statuses.Values.FirstOrDefault(s => s.Id == exposureStatusId);
            Status userStatus = statuses.Values.FirstOrDefault(s => s.Id == userState.StatusId);

            if (exposureStatus == null)
            {
                throw new ApplicationException("Invalid configuration: Statuses.OnExposure not found");
            }

            if (userStatus == null)
            {
                throw new ApplicationException("User status not found");
            }

            if (userStatus.Severity >= exposureStatus.Severity)
            {
                logger.LogInformation(
                    "ContactToken: {contactToken}: Status: {contactStatus}. No need change user status",
                    exposedContact.UserToken,
                    userStatus.Id);

                return;
            }

            // set status to AtRisk
            var userStateStatus = new UserStateStatus()
            {
                StatusChangedOn = DateTime.UtcNow,
                UserToken = userState.UserToken,
                StatusId = exposureStatusId
            };
            userRepository.SetUserStatus(userStateStatus);
            logger.LogInformation(
                "ContactToken: {contactToken}: Status: {contactStatusId}. Status changed to {exposureStatusId}",
                exposedContact.UserToken,
                userStatus.Id,
                exposureStatusId);

            // send Notification to ASB
            if (string.IsNullOrEmpty(userState.NotificationToken) ||
                string.IsNullOrEmpty(userState.NotificationTarget))
            {
                logger.LogWarning(
                    "ContactToken: {contactToken}. Push notification message not sent to ASB",
                    exposedContact.UserToken);

                return;
            }

            var pushNotification = new PushNotification()
            {
                Handle = userState.NotificationToken,
                Platform = userState.NotificationTarget,
                Message = backendOptions.Value.Infection.ExposureNotification
            };

            var messageProperties = new Dictionary<string, object>
            {
                { MessageHeaders.Subject, "StatusNotification" },
                { MessageHeaders.From, "ExposedContactJob" },
                { "UserToken", exposedContact.UserToken }
            };

            Message pushMessage = MessageHelper.CreateMessageFromObject(pushNotification, messageProperties);

            await MessageSender.SendAsync(pushMessage);
            logger.LogInformation(
                "ContactToken: {contactToken}. Push notification message is sent to ASB",
                exposedContact.UserToken);
        }
    }
}
