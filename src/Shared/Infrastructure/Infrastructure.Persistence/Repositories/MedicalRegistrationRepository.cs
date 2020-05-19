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
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using System;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories
{
    public interface IMedicalRegistrationRepository
    {
        bool TryCreateSecurityId(string securityId, string comment);

        bool TryRegistration(string securityId);

        void RollBackRegistration(string securityId);
    }

    public class MedicalRegistrationRepository : IMedicalRegistrationRepository
    {
        private readonly ISession session;
        private readonly Mapper mapper;

        public MedicalRegistrationRepository(ICassandraSession session)
        {
            this.session = session.Session;
            mapper = new Mapper(this.session);
        }

        public bool TryCreateSecurityId(string securityId, string comment)
        {
            var createRequest = new MedicalRegistrationCreate
            {
                HealthSecurityId = securityId,
                Comment = comment
            };
            return mapper.InsertIfNotExists(createRequest).Applied;
        }

        public bool TryRegistration(string securityId)
        {
            MedicalRegistration registration = new Table<MedicalRegistration>(session)
                .FirstOrDefault(row => row.HealthSecurityId == securityId)
                .Execute();

            if (registration == null || registration.TakenOn != null)
            {
                return false;
            }

            //return new Table<MedicalRegistrationTake>(session)
            //    .Where(row => row.Code == code)
            //    .Select(row => new MedicalRegistrationTake { TakenOn = DateTime.UtcNow })
            //    .UpdateIf(row => row.TakenOn == null)
            //    .Execute()
            //    .Applied;

            new Table<MedicalRegistrationTake>(session)
                .Where(row => row.HealthSecurityId == securityId)
                .Select(row => new MedicalRegistrationTake { TakenOn = DateTime.UtcNow })
                .Update()
                .Execute();
            return true;
        }

        public void RollBackRegistration(string securityId)
            => new Table<MedicalRegistrationTake>(session)
                .Where(row => row.HealthSecurityId == securityId)
                .Select(row => new MedicalRegistrationTake { TakenOn = null })
                .Update()
                .Execute();
    }
}
