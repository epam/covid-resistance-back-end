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

using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.IntegrationTests
{
    [TestFixture]
    public class MedicalRegistrationRepositoryTests : TestsBase
    {
        [Test]
        public void TestMedicalRegistration()
        {
            var repository = ServiceProvider.GetService<IMedicalRegistrationRepository>();
            const string securityId = "TestSecurityId";

            Assert.That(repository.TryCreateSecurityId(securityId, "comment1"), Is.True, "Creating a new securityId succeeds");
            Assert.That(repository.TryCreateSecurityId(securityId, "comment2"), Is.False, "Re-creating the same securityId fails");

            Assert.That(repository.TryRegistration(securityId), Is.True, "Registration with a good security id succeeds");
            Assert.That(repository.TryRegistration(securityId), Is.False, "Registration with a taken security id fails");
            Assert.That(repository.TryRegistration(securityId + "missing"), Is.False, "Registration with a missing security id fails");
        }
    }
}
