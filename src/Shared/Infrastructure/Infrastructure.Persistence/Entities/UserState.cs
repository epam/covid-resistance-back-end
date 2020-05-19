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
    public class UserState
    {
        public string UserToken { get; set; }
        public string UserName { get; set; }
        public string NotificationToken { get; set; }
        public string NotificationTarget { get; set; }
        public int StatusId { get; set; }
        public DateTime StatusChangedOn { get; set; }
    }

    public class UserStateStatus
    {
        public string UserToken { get; set; }
        public int StatusId { get; set; }
        public DateTime StatusChangedOn { get; set; }
    }

    public class UserStatePushNotification
    {
        public string UserToken { get; set; }
        public string NotificationToken { get; set; }
        public string NotificationTarget { get; set; }
    }

    public class UserStateMappings : Mappings
    {
        private const string TableName = "UserState";
        public UserStateMappings()
        {
            For<UserState>()
                .TableName(TableName)
                .PartitionKey(token => token.UserToken);
            For<UserStateStatus>()
                .TableName(TableName)
                .PartitionKey(token => token.UserToken);
            For<UserStatePushNotification>()
                .TableName(TableName)
                .PartitionKey(token => token.UserToken);
        }
    }
}
