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

using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Text;

namespace Epam.CovidResistance.Services.Infection.FnApp.Helpers
{
    /// <summary>
    /// The message header names.
    /// </summary>
    public static class MessageHeaders
    {
        /// <summary>The Subject header name</summary>
        public const string Subject = "Subject";

        /// <summary>The From header name</summary>
        public const string From = "From";

        /// <summary>The UserToken header name</summary>
        public const string UserToken = "UserToken";
    }

    /// <summary>
    /// Represents helper with methods to manipulate service bus message.
    /// </summary>
    public static class MessageHelper
    {
        private const string ContentType = "application/json";

        /// <summary>Creates the object from message.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static T GetMessageBody<T>(Message message)
        {
            var jsonContent = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);
            var data = JsonConvert.DeserializeObject<T>(jsonContent);
            return data;
        }

        /// <summary>Creates the message from object.</summary>
        /// <param name="content">The content.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static Message CreateMessageFromObject(object content, IDictionary<string, object> properties, string sessionId = null)
        {
            var serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            var payload = JsonConvert.SerializeObject(content, serializerSettings);
            var message = new Message(Encoding.UTF8.GetBytes(payload))
            {
                ContentType = ContentType
            };

            if (properties != null)
            {
                foreach (KeyValuePair<string, object> property in properties)
                {
                    message.UserProperties.Add(property);
                }
            }

            if (sessionId != null)
            {
                message.SessionId = sessionId;
            }

            return message;
        }
    }
}
