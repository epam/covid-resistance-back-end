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

using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.IntegrationTests
{
    [TestFixture]
    public class MeetingRepositoryTests : TestsBase
    {
        [Test]
        public void TestMeetings()
        {
            var contact1 = new MeetingContact(new DateTime(2020, 4, 1), "userToken1");
            var contact3 = new MeetingContact(new DateTime(2020, 4, 3), "userToken2");
            var contact4 = new MeetingContact(new DateTime(2020, 4, 4), "userToken2");
            var contact6 = new MeetingContact(new DateTime(2020, 4, 6), "userToken1");
            var allMeetingContacts = new List<MeetingContact> { contact1, contact3, contact4, contact6 };

            var meetingRepository = ServiceProvider.GetService<IMeetingRepository>();
            meetingRepository.Save("owner", allMeetingContacts);

            var betweenTimestamps = meetingRepository.GetContacts(
                userToken: "owner",
                afterTimestamp: new DateTime(2020, 4, 2),
                beforeTimestamp: new DateTime(2020, 4, 5));
            Assert.That(betweenTimestamps, Is.EquivalentTo(new List<MeetingContact> { contact3, contact4 }));
        }
    }
}