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

using Cassandra.Data.Linq;
using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.IntegrationTests
{
    [TestFixture]
    public class UserRepositoryTests : TestsBase
    {
        [Test]
        public void TestRegistration()
        {
            var repository = ServiceProvider.GetService<IUserRepository>();
            var userInfo = new UserInfo
                (
                    userToken: "registrationToken1",
                    userName: "joe",
                    statusId: 1,
                    statusChangedOn: new DateTime(2020, 4, 1)
                );

            repository.RegisterUser(userInfo);

            Assert.That(repository.GetUserInfo(userInfo.UserToken), Is.EqualTo(userInfo));
            var userStateHistory = new Table<UserStateHistory>(CassandraSession.Session)
                .FirstOrDefault(row => row.UserToken == userInfo.UserToken && row.StatusChangedOn == userInfo.StatusChangedOn)
                .Execute();

            Assert.That(userStateHistory, Is.Not.Null);
            Assert.That(userStateHistory.StatusId, Is.EqualTo(userInfo.StatusId));
            Assert.That(userStateHistory.StatusChangedOn, Is.EqualTo(userInfo.StatusChangedOn));
        }

        [Test]
        public void TestPushNotifications()
        {
            var repository = ServiceProvider.GetService<IUserRepository>();

            var pushNotification = new UserStatePushNotification
            {
                UserToken = "userTokenForPushNotification",
                NotificationToken = "pushToken",
                NotificationTarget = "pushTarget"
            };
            repository.SetPushNotification(pushNotification);

            Assert.That(repository.GetPushNotification("missing"), Is.Null);
            var actualPushNotification = repository.GetPushNotification(pushNotification.UserToken);
            Assert.That(actualPushNotification.NotificationToken, Is.EqualTo(pushNotification.NotificationToken));
            Assert.That(actualPushNotification.NotificationTarget, Is.EqualTo(pushNotification.NotificationTarget));
        }

        [Test]
        public void TestUserStatus()
        {
            var repository = ServiceProvider.GetService<IUserRepository>();
            var userInfo = new UserInfo
                (
                    userToken: "registrationToken1",
                    userName: "joe",
                    statusId: 1,
                    statusChangedOn: new DateTime(2020, 4, 1)
                );

            repository.RegisterUser(userInfo);
            var userStatus = new UserStateStatus
            {
                UserToken = userInfo.UserToken,
                StatusId = 2,
                StatusChangedOn = new DateTime(2020, 4, 4)
            };
            repository.SetUserStatus(userStatus);

            var newUserInfo = repository.GetUserInfo(userInfo.UserToken);

            Assert.That(newUserInfo, Is.Not.EqualTo(userInfo));
            Assert.That(newUserInfo.StatusId, Is.EqualTo(userStatus.StatusId));
            Assert.That(newUserInfo.StatusChangedOn, Is.EqualTo(userStatus.StatusChangedOn));

            var userStateHistory = new Table<UserStateHistory>(CassandraSession.Session)
                .FirstOrDefault(row => row.UserToken == userStatus.UserToken && row.StatusChangedOn == userStatus.StatusChangedOn)
                .Execute();

            Assert.That(userStateHistory, Is.Not.Null);
            Assert.That(userStateHistory.StatusId, Is.EqualTo(userStatus.StatusId));
            Assert.That(userStateHistory.StatusChangedOn, Is.EqualTo(userStatus.StatusChangedOn));
        }

        [Test]
        public void TestUserInfo()
        {
            var repository = ServiceProvider.GetService<IUserRepository>();
            var user = new UserInfo("userInfoToken1", "userInfoName1", 1, new DateTime(2020, 4, 1));
            repository.RegisterUser(user);

            Assert.That(repository.GetUserInfo(user.UserToken), Is.EqualTo(user));
            Assert.That(repository.GetUserInfo("missingToken"), Is.Null);
        }
    }
}
