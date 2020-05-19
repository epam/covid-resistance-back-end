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

using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories
{
    public interface IMeetingRepository
    {
        void Save(string token, IEnumerable<MeetingContact> contacts);
        IList<MeetingContact> GetContacts(string userToken, DateTime afterTimestamp, DateTime beforeTimestamp);
    }

    public class MeetingRepository : IMeetingRepository
    {
        private readonly ISession session;
        private readonly IMapper mapper;
        public MeetingRepository(ICassandraSession session)
        {
            this.session = session.Session;
            mapper = new Mapper(this.session);
        }

        public void Save(string token, IEnumerable<MeetingContact> contacts)
        {
            var meetings = contacts.Select(contact => contact.ToMeeting(token));
            foreach (var meeting in meetings)
            {
                mapper.Insert<Meeting>(meeting);
            }
        }

        public IList<MeetingContact> GetContacts(string userToken, DateTime afterTimestamp, DateTime beforeTimestamp)
        {
            return new Table<Meeting>(session)
                .Where(m => m.OwnerUserToken == userToken && m.MeetingTime >= afterTimestamp && m.MeetingTime <= beforeTimestamp)
                .Execute()
                .Select(m => new MeetingContact(m.MeetingTime, m.UserToken))
                .ToList();
        }
    }
}