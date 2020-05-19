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
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Response;
using System;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories
{
    public interface IStatusChangeRepository
    {
        bool TryCreateChangeRequest(string medicalCode, StatusChangeRequest statusChangeRequest, string medicalUserToken);

        AcceptChangeResponse TryAcceptChangeRequest(string medicalCode, int expirationHours);
    }

    public class StatusChangeRepository : IStatusChangeRepository
    {
        private readonly ISession session;
        private readonly Mapper mapper;

        public StatusChangeRepository(ICassandraSession session)
        {
            this.session = session.Session;
            mapper = new Mapper(this.session);
        }

        public bool TryCreateChangeRequest(string medicalCode, StatusChangeRequest statusChangeRequest, string medicalUserToken)
        {
            var statusChange = new StatusChange
            {
                MedicalCode = medicalCode,
                StatusId = statusChangeRequest.StatusId,
                StatusChangedOn = statusChangeRequest.StatusChangedOn,
                Comment = statusChangeRequest.Comment,
                CreatedAt = DateTime.UtcNow,
                RequestedBy = medicalUserToken
            };
            return mapper.InsertIfNotExists(statusChange).Applied;
        }

        public AcceptChangeResponse TryAcceptChangeRequest(string medicalCode, int expirationHours)
        {
            var statusChange = new Table<StatusChange>(session)
                .FirstOrDefault(row => row.MedicalCode == medicalCode)
                .Execute();

            if (statusChange == null)
            {
                return AcceptChangeResponse.Error(AcceptChangeResult.MissingCode);
            }
            if (statusChange.AcceptedAt != null)
            {
                return AcceptChangeResponse.Error(AcceptChangeResult.ReusedCode);
            }

            if (statusChange.CreatedAt.AddHours(expirationHours) < DateTime.UtcNow)
            {
                return AcceptChangeResponse.Error(AcceptChangeResult.ExpiredCode);
            }

            // CosmosDB doesn't support this Lightweight Transaction
            //return new Table<StatusChangeAccepted>(session)
            //    .Where(row => row.MedicalCode == medicalCode)
            //    .Select(row => new StatusChangeAccepted { AcceptedAt = DateTime.UtcNow })
            //    .UpdateIf(row => row.AcceptedAt == null)
            //    .Execute()
            //    .Applied;

            new Table<StatusChangeAccepted>(session)
                .Where(row => row.MedicalCode == medicalCode)
                .Select(row => new StatusChangeAccepted { AcceptedAt = DateTime.UtcNow })
                .Update()
                .Execute();

            return AcceptChangeResponse.Success(statusChange.StatusId, statusChange.StatusChangedOn);
        }
    }
}
