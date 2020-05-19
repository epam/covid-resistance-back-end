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

using AutoMapper;
using Epam.CovidResistance.Services.Infection.FnApp.Entities;
using Epam.CovidResistance.Services.Infection.FnApp.Helpers;
using Epam.CovidResistance.Services.Infection.FnApp.Interfaces;
using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
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
    /// The contact tracing service.
    /// </summary>
    public class ContactTracingService : IContactTracingService
    {
        private readonly IMapper mapper;
        private readonly IMeetingRepository meetingRepository;
        private readonly IUserRepository userRepository;
        private readonly IOptions<Backend> backendOptions;
        private readonly ILogger<ContactTracingService> logger;

        /// <summary>Gets or sets the message sender.</summary>
        /// <value>The message sender.</value>
        public MessageSender MessageSender { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ContactTracingService" /> class.</summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="meetingRepository"></param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="backendOptions">The backend options.</param>
        /// <param name="logger">The logger.</param>
        public ContactTracingService(
            IMapper mapper,
            IMeetingRepository meetingRepository,
            IUserRepository userRepository,
            IOptions<Backend> backendOptions,
            ILogger<ContactTracingService> logger)
        {
            this.mapper = mapper;
            this.meetingRepository = meetingRepository;
            this.userRepository = userRepository;
            this.backendOptions = backendOptions;
            this.logger = logger;
        }

        /// <summary>Processes the contact list.</summary>
        /// <param name="userToken">The user token.</param>
        /// <param name="meetings">The meetings.</param>
        public async Task ProcessContactList(string userToken, IList<Meeting> meetings)
        {
            // Push all to Meeting repository
            // This functionality is commented out and not required in MVP. 
            // List<MeetingContact> meetingsDtoList = meetings
            //    .Select(meeting => mapper.Map<MeetingContact>(meeting))
            //    .ToList();
            // meetingRepository.Save(userToken, meetingsDtoList);
            // logger.LogInformation("UserToken: {userToken}. {meetingsCount} meetings are saved to DB", userToken, meetingsDtoList.Count);

            UserInfo userInfo = userRepository.GetUserInfo(userToken);
            DateTime infectedDate = userInfo.StatusChangedOn;
            logger.LogInformation(
                "UserToken: {userToken}. Status: {userStatusId}, StatusDate: {userStatusChangedOn}",
                userToken,
                userInfo.StatusId,
                userInfo.StatusChangedOn);

            // filter contacts
            // uses the current user status as Infected!!!
            // Example: If the doctor sets 2020.03.15 as infection date, we should query the contacts for 14 days both ways (2020.03.01 – 2020.03.29) 
            var lookupDays = backendOptions.Value.Infection.ContactLookupDays;
            IEnumerable<Meeting> exposedMeetings = meetings
                .Where(m => (m.Timestamp >= infectedDate.AddDays(lookupDays * -1) && m.Timestamp <= infectedDate.AddDays(lookupDays)));

            IEnumerable<Meeting> distinctExposedMeetings = from x in exposedMeetings
                                                           group x by x.UserToken into g
                                                           select new Meeting { UserToken = g.Key, Timestamp = g.Max(a => a.Timestamp) };

            // push exposed contact to service bus topic
            var messageProperties = new Dictionary<string, object>
            {
                { MessageHeaders.Subject, "ExposedContact" },
                { MessageHeaders.From, "ContactTracingJob" },
                { "UserToken", userToken }
            };
            List<ExposedContact> exposedContacts = distinctExposedMeetings
                .Select(meeting => mapper.Map<ExposedContact>(meeting))
                .ToList();

            foreach (ExposedContact exposedContact in exposedContacts)
            {
                Message message = MessageHelper.CreateMessageFromObject(exposedContact, messageProperties);
                await MessageSender.SendAsync(message);
            }

            logger.LogInformation(
                "UserToken: {userToken}. {exposedContactsCount} exposed contacts are sent to ASB",
                userToken,
                exposedContacts.Count);
        }
    }
}
