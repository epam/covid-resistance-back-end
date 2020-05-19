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

using Epam.CovidResistance.Services.Notification.FnApp.Exceptions;
using Epam.CovidResistance.Services.Notification.FnApp.Options;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Notification.FnApp.Services
{

    /// <summary>
    /// The retry handler.
    /// </summary>
    public static class RetryHandler
    {
        /// <summary>Retries the message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageReceiver">The message receiver.</param>
        /// <param name="policyOptions">The policy options.</param>
        /// <param name="logger">The logger.</param>
        public static async Task<bool> RetryMessageAsync(
            Message message,
            Exception exception,
            MessageReceiver messageReceiver,
            RetryPolicyOptions policyOptions,
            ILogger logger)
        {
            logger.LogInformation($"Calling retry handler... DeliveryCount: {message.SystemProperties.DeliveryCount}");

            RetryPolicyOptions policy = policyOptions;

            if (exception is BadMessageException ||
                exception is ApplicationException ||
                exception is NotSupportedException)
            {
                // Business errors policy
                // Move message to dead letter queue
                await messageReceiver.DeadLetterAsync(
                    message.SystemProperties.LockToken,
                    "Application error",
                    exception.Message);

                logger.LogInformation("RetryHandler: Application error. Message moved to dead letter queue.");

                return true;
            }

            // Check message delivery count against policy
            if (message.SystemProperties.DeliveryCount >= policy?.MaxRetryCount)
            {
                // Move message to dead letter queue
                await messageReceiver.DeadLetterAsync(
                    message.SystemProperties.LockToken,
                    "Exceeded max retry policy",
                    exception.Message);

                logger.LogInformation(
                    $"RetryHandler: Max delivery attempts {message.SystemProperties.DeliveryCount} exceeded. Message moved to dead letter queue");

                return true;
            }

            return false;
        }
    }
}