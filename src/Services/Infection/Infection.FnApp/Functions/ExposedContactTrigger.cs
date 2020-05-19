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
using Epam.CovidResistance.Services.Infection.FnApp.Options;
using Epam.CovidResistance.Services.Infection.FnApp.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.FnApp.Functions
{
    /// <summary>
    /// Represents a service bus trigger to new message from ExposedContact subscription
    /// </summary>
    public class ExposedContactTrigger
    {
        private readonly RetryPolicyOptions policyOptions;

        private readonly IExposedContactService infectionService;
        private readonly ILogger<ExposedContactTrigger> logger;

        /// <summary>Initializes a new instance of the <see cref="ExposedContactTrigger" /> class.</summary>
        /// <param name="policyOptions">The policy options.</param>
        /// <param name="infectionService">The infection service.</param>
        /// <param name="logger">The logger.</param>
        public ExposedContactTrigger(
            IOptions<RetryPolicyOptions> policyOptions,
            IExposedContactService infectionService,
            ILogger<ExposedContactTrigger> logger)
        {
            this.policyOptions = policyOptions.Value;
            this.infectionService = infectionService;
            this.logger = logger;
        }

        /// <summary>Runs the specified ServiceBus trigger.</summary>
        /// <param name="message">The message.</param>
        /// <param name="messageReceiver">The message receiver.</param>
        /// <param name="messagesQueue">The messages queue.</param>
        /// <exception cref="ApplicationException">The UserToken is required
        /// or
        /// The MeetingTime is required</exception>
        [FunctionName(nameof(ExposedContactTrigger))]
        public async Task Run(
            [ServiceBusTrigger("%ContactsTopicName%", "%ExposedContactsSubscription%", Connection = "ContactsTopicListenConnection")]Message message,
            MessageReceiver messageReceiver,
            [ServiceBus("%NotificationsTopicName%", Connection = "NotificationTopicSendConnection")] MessageSender messagesQueue)
        {
            logger.LogInformation($"C# ServiceBus topic trigger function processed message: {message}");

            try
            {
                message.UserProperties.TryGetValue(MessageHeaders.UserToken, out object userTokenHeader);
                var infectedUserToken = userTokenHeader?.ToString();

                var exposedContact = MessageHelper.GetMessageBody<ExposedContact>(message);

                if (string.IsNullOrEmpty(exposedContact.UserToken))
                {
                    throw new ApplicationException("The UserToken is required");
                }

                if (exposedContact.MeetingTime == DateTime.MinValue)
                {
                    throw new ApplicationException("The MeetingTime is required");
                }

                logger.LogInformation(
                    "UserToken: {userToken}. Exposed contact {contactToken} received.",
                    infectedUserToken,
                    exposedContact.UserToken);

                infectionService.MessageSender = messagesQueue;
                await infectionService.ProcessContact(exposedContact);

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
                    logger.LogError($"Unhandled exception: {ex.Message}", ex);
                    throw;
                }
            }
        }
    }
}
