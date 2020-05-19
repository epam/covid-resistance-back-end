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
using Epam.CovidResistance.Services.Notification.FnApp.Exceptions;
using Epam.CovidResistance.Services.Notification.FnApp.Helpers;
using Epam.CovidResistance.Services.Notification.FnApp.Interfaces;
using Epam.CovidResistance.Services.Notification.FnApp.Options;
using Epam.CovidResistance.Services.Notification.FnApp.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Notification.FnApp.Functions
{
    /// <summary>
    /// The service bus trigger to new message from Notification topic.
    /// </summary>
    public class NotificationTrigger
    {
        private readonly RetryPolicyOptions policyOptions;

        private readonly INotificationsService notificationsService;
        private readonly ILogger<NotificationTrigger> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTrigger"/> class.
        /// </summary>
        /// <param name="policyOptions">
        /// The policy options.
        /// </param>
        /// <param name="notificationsService">
        /// The notifications service.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public NotificationTrigger(
            IOptions<RetryPolicyOptions> policyOptions,
            INotificationsService notificationsService,
            ILogger<NotificationTrigger> logger)
        {
            this.policyOptions = policyOptions.Value;
            this.notificationsService = notificationsService;
            this.logger = logger;
        }

        /// <summary>Runs the specified service bus message trigger.</summary>
        /// <param name="message">The message.</param>
        /// <param name="messageReceiver">The message receiver.</param>
        /// <exception cref="Epam.CovidResistance.Services.Notification.FnApp.Exceptions.BadMessageException">Invalid message received.</exception>
        [FunctionName(nameof(NotificationTrigger))]
        public async Task Run(
            [ServiceBusTrigger("%NotificationsTopicName%", "%StatusNotificationSubscription%", Connection = "NotificationsTopicListenConnection")]Message message,
            MessageReceiver messageReceiver)
        {
            logger.LogInformation($"C# ServiceBus topic trigger function processed message: {message}");

            try
            {
                message.UserProperties.TryGetValue(MessageHeaders.UserToken, out object userTokenHeader);
                var contactToken = userTokenHeader?.ToString();

                var pushNotification = MessageHelper.GetMessageBody<PushNotification>(message);

                if (pushNotification == null || !pushNotification.IsValid())
                {
                    throw new BadMessageException("Invalid message received.");
                }

                logger.LogInformation("ContactToken: {contactToken}. Sending notification...", contactToken);
                await notificationsService.PushMessageAsync(pushNotification);

                await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                // Manage retries using our message retry handler
                if (!await RetryHandler.RetryMessageAsync(
                         message,
                         ex,
                         messageReceiver,
                         policyOptions,
                         logger))
                {
                    logger.LogError(ex, $"Unhandled exception: {ex.Message}");
                    throw;
                }
            }
        }
    }
}