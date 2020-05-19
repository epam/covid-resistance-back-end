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
    public class Meeting
    {
        public string OwnerUserToken { get; set; }
        public DateTime MeetingTime { get; set; }
        public string UserToken { get; set; }
    }

    public class MeetingMappings : Mappings
    {
        private const string TableName = "Meeting";
        public MeetingMappings()
        {
            For<Meeting>()
                .TableName(TableName)
                .PartitionKey(m => m.OwnerUserToken)
                .ClusteringKey(m => m.MeetingTime);
        }
    }
}
