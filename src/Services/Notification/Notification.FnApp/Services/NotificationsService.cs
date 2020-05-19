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

using Epam.CovidResistance.Services.Notification.FnApp.Entities;
using Epam.CovidResistance.Services.Notification.FnApp.Interfaces;
using Epam.CovidResistance.Services.Notification.FnApp.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Notification.FnApp.Services
{
    /// <summary>
    /// The notifications service.
    /// </summary>
    public class NotificationsService : INotificationsService
    {
        private readonly ILogger<NotificationsService> logger;

        private readonly NotificationHubClient nhClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsService"/> class.
        /// </summary>
        /// <param name="notificationHubOptions">
        /// The notification hub options.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public NotificationsService(
            IOptions<NotificationHubOptions> notificationHubOptions,
            ILogger<NotificationsService> logger)
        {
            this.logger = logger;
            nhClient = NotificationHubClient.CreateClientFromConnectionString(notificationHubOptions.Value.Connection, notificationHubOptions.Value.Name);
        }

        /// <summary>Pushes the message asynchronous.</summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Unknown PNS type</exception>
        public async Task<IActionResult> PushMessageAsync(PushNotification request)
        {
            NotificationOutcome outcome;
            // Send notifications by deviceId
            switch (request.Platform.ToLower())
            {
                case "fcm":
                    {
                        outcome = await nhClient.SendDirectNotificationAsync(CreateFcmNotification(request.Message), request.Handle);
                        break;
                    }
                case "apns":
                    {
                        outcome = await nhClient.SendDirectNotificationAsync(CreateApnsNotification(request.Message), request.Handle);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("Unknown PNS type");
                    }
            }

            logger.LogInformation($"Send notification result: {outcome.State}; Success: {outcome.Success}");

            return new OkResult();
        }

        private static Microsoft.Azure.NotificationHubs.Notification CreateFcmNotification(string message)
        {
            var jObj = new JObject(new JProperty("data", new JObject(new JProperty("message", message))));
            return new FcmNotification(jObj.ToString());
        }

        private static Microsoft.Azure.NotificationHubs.Notification CreateApnsNotification(string message)
        {
            var jObj = new JObject(new JProperty("aps", new JObject(new JProperty("alert", message))));
            return new AppleNotification(jObj.ToString());
        }
    }
}