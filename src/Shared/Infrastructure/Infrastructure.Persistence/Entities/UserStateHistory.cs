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
    public class UserStateHistory
    {
        public string UserToken { get; set; }
        public DateTime StatusChangedOn { get; set; }
        public int StatusId { get; set; }
    }

    public class UserStateHistoryMappings : Mappings
    {
        private const string TableName = "UserStateHistory";
        public UserStateHistoryMappings()
        {
            For<UserStateHistory>()
                .TableName(TableName)
                .PartitionKey(row => row.UserToken)
                .ClusteringKey(row => row.StatusChangedOn);
        }
    }
}