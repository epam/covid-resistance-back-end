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
using Epam.CovidResistance.Services.Infection.FnApp.Interfaces;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.FnApp.Functions
{
    /// <summary>
    /// The contact tracing trigger.
    /// </summary>
    public class ContactTracingTrigger
    {
        /// <summary>
        /// The user token name.
        /// </summary>
        private const string UserToken = "UserToken";

        /// <summary>
        /// The contact tracing service.
        /// </summary>
        private readonly IContactTracingService contactTracingService;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<ContactTracingTrigger> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactTracingTrigger"/> class.
        /// </summary>
        /// <param name="contactTracingService">
        /// The contact tracing service.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public ContactTracingTrigger(
            IContactTracingService contactTracingService,
            ILogger<ContactTracingTrigger> logger)
        {
            this.contactTracingService = contactTracingService;
            this.logger = logger;
        }

        /// <summary>Runs the specified BLOB trigger.</summary>
        /// <param name="myBlob">My BLOB.</param>
        /// <param name="name">The name.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="messagesQueue">The messages queue.</param>
        /// <exception cref="Exception">Invalid meeting list format</exception>
        [FunctionName(nameof(ContactTracingTrigger))]
        public async Task Run(
            [BlobTrigger("%BlobPath%/{name}", Connection = "BlobConnection")]Stream myBlob,
            string name,
            IDictionary<string, string> metadata,
            [ServiceBus("%ContactsTopicName%", Connection = "ContactsTopicSendConnection")] MessageSender messagesQueue)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            try
            {
                // extract userToken
                var userToken = string.Empty;
                metadata?.TryGetValue(UserToken, out userToken);

                // extract meetings
                var reader = new StreamReader(myBlob);
                var jsonContent = await reader.ReadToEndAsync();
                JObject jsonObject = JObject.Parse(jsonContent);

                IList<Meeting> meetings = null;
                JToken trgArray = jsonObject.Descendants().First(d => d is JArray);
                if (trgArray != null && trgArray.Type == JTokenType.Array)
                {
                    meetings = JsonConvert.DeserializeObject<List<Meeting>>(trgArray.ToString());
                }

                if (meetings == null)
                {
                    throw new Exception("Invalid meeting list format");
                }

                logger.LogInformation("UserToken: {userToken}. Received {meetingsCount} meetings", userToken, meetings.Count);

                contactTracingService.MessageSender = messagesQueue;
                await contactTracingService.ProcessContactList(userToken, meetings);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unhandled exception: {ex.Message}");
                throw;
            }
        }
    }
}
