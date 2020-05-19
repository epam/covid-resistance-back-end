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

using Cassandra.Mapping;
using System;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities
{
    public class MedicalRegistration
    {
        public string HealthSecurityId { get; set; }
        public string Comment { get; set; }
        public DateTime? TakenOn { get; set; }
    }

    public class MedicalRegistrationCreate
    {
        public string HealthSecurityId { get; set; }
        public string Comment { get; set; }
    }

    public class MedicalRegistrationTake
    {
        public string HealthSecurityId { get; set; }
        public DateTime? TakenOn { get; set; }
    }

    public class MedicalRegistrationMappings : Mappings
    {
        private const string TableName = "MedicalRegistrationCode";

        public MedicalRegistrationMappings()
        {
            For<MedicalRegistration>()
                .TableName(TableName)
                .PartitionKey(r => r.HealthSecurityId);
            For<MedicalRegistrationCreate>()
                .TableName(TableName)
                .PartitionKey(r => r.HealthSecurityId);
            For<MedicalRegistrationTake>()
                .TableName(TableName)
                .PartitionKey(r => r.HealthSecurityId);
        }
    }
}

