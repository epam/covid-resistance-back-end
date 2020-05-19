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
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Response;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.IntegrationTests
{
    [TestFixture]
    public class StatusChangeRepositoryTests : TestsBase
    {
        [Test]
        public void TestStatusChangeRequest()
        {
            var medicalCode = "XYZ123";
            var infectedStatusId = 1;
            var repository = ServiceProvider.GetService<IStatusChangeRepository>();
            var changeRequest = new StatusChangeRequest(infectedStatusId, new DateTime(2020, 4, 1), "Comment");

            var firstRequestSuccessful = repository.TryCreateChangeRequest(medicalCode, changeRequest, "medicalUserToken");
            Assert.That(firstRequestSuccessful, Is.True);

            var secondRequestSuccessful = repository.TryCreateChangeRequest(medicalCode, changeRequest, "otherDoctor");
            Assert.That(secondRequestSuccessful, Is.False);

            var statusChange = new Table<StatusChange>(CassandraSession.Session)
                .FirstOrDefault(row => row.MedicalCode == medicalCode)
                .Execute();

            Assert.That(statusChange, Is.Not.Null);
            Assert.That(statusChange.AcceptedAt, Is.Null);
            Assert.That(statusChange.Comment, Is.EqualTo(changeRequest.Comment));
            Assert.That(statusChange.CreatedAt, Is.Not.Null);
            Assert.That(statusChange.RequestedBy, Is.EqualTo("medicalUserToken"));
            Assert.That(statusChange.StatusId, Is.EqualTo(changeRequest.StatusId));
            Assert.That(statusChange.StatusChangedOn, Is.EqualTo(changeRequest.StatusChangedOn));
        }

        [Test]
        public void TestStatusChangeAccept()
        {
            var medicalCode = "ABC123";
            var infectedStatusId = 1;
            var repository = ServiceProvider.GetService<IStatusChangeRepository>();
            var changeRequest = new StatusChangeRequest(infectedStatusId, new DateTime(2020, 4, 1), "Comment");

            Assert.That(repository.TryCreateChangeRequest(medicalCode, changeRequest, "medicalUserToken"),
                Is.True, "Creating a new code is successful");

            Assert.That(repository.TryAcceptChangeRequest(medicalCode + "invalid", 24),
                Is.EqualTo(AcceptChangeResponse.Error(AcceptChangeResult.MissingCode)), "Accepting with missing code fails");

            Assert.That(repository.TryAcceptChangeRequest(medicalCode, 0),
                Is.EqualTo(AcceptChangeResponse.Error(AcceptChangeResult.ExpiredCode)), "Accepting after expiration fails");

            Assert.That(repository.TryAcceptChangeRequest(medicalCode, 24),
                Is.EqualTo(AcceptChangeResponse.Success(infectedStatusId, changeRequest.StatusChangedOn)),
                "Accepting with good code succeeds");

            // verify that AcceptedAt is set
            var statusChange = new Table<StatusChange>(CassandraSession.Session)
                .FirstOrDefault(row => row.MedicalCode == medicalCode)
                .Execute();
            Assert.That(statusChange.AcceptedAt, Is.Not.Null);

            Assert.That(
                repository.TryAcceptChangeRequest(medicalCode, 24),
                Is.EqualTo(AcceptChangeResponse.Error(AcceptChangeResult.ReusedCode)), "Accepting with a taken code fails");
        }
    }
}
