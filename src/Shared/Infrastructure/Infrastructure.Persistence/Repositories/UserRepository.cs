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

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories
{
    public interface IUserRepository
    {
        void RegisterUser(UserInfo userInfo);
        void SetUserStatus(UserStateStatus userStateStatus);
        void SetPushNotification(UserStatePushNotification pushNotification);
        UserStatePushNotification GetPushNotification(string userToken);
        UserInfo GetUserInfo(string userToken);
        UserState GetUserState(string userToken);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ISession session;
        private readonly Mapper mapper;

        public UserRepository(ICassandraSession session)
        {
            this.session = session.Session;
            mapper = new Mapper(this.session);
        }

        public void RegisterUser(UserInfo userInfo)
        {
            var userState = new UserState
            {
                UserToken = userInfo.UserToken,
                UserName = userInfo.UserName,
                StatusId = userInfo.StatusId,
                StatusChangedOn = userInfo.StatusChangedOn
            };
            mapper.Insert(userState);
            var userStateHistory = new UserStateHistory
            {
                UserToken = userInfo.UserToken,
                StatusChangedOn = userInfo.StatusChangedOn,
                StatusId = userInfo.StatusId
            };
            mapper.Insert(userStateHistory);
        }

        public void SetUserStatus(UserStateStatus userStateStatus)
        {
            mapper.Insert(userStateStatus);
            var userStateHistory = new UserStateHistory
            {
                UserToken = userStateStatus.UserToken,
                StatusChangedOn = userStateStatus.StatusChangedOn,
                StatusId = userStateStatus.StatusId
            };
            mapper.Insert(userStateHistory);
        }

        public void SetPushNotification(UserStatePushNotification pushNotification)
        {
            mapper.Insert(pushNotification);
        }

        public UserStatePushNotification GetPushNotification(string userToken)
        {
            return new Table<UserStatePushNotification>(session)
                .FirstOrDefault(token => token.UserToken == userToken)
                .Execute();
        }

        public UserState GetUserState(string userToken)
        {
            return new Table<UserState>(session)
                .FirstOrDefault(u => u.UserToken == userToken)
                .Execute();
        }

        public UserInfo GetUserInfo(string userToken)
        {
            UserState userState = GetUserState(userToken);

            return userState == null
                       ? null
                       : new UserInfo(
                           userToken: userState.UserToken,
                           userName: userState.UserName,
                           statusId: userState.StatusId,
                           statusChangedOn: userState.StatusChangedOn
                       );
        }
    }
}
