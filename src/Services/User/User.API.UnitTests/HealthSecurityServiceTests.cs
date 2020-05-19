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

using Epam.CovidResistance.Services.User.API.Interfaces;
using Epam.CovidResistance.Services.User.API.Services;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;

namespace Epam.CovidResistance.Services.User.API.UnitTests
{
    public class HealthSecurityServiceTests
    {
        [Test]
        public void ShouldGenerateSecurityIds()
        {
            var repository = Substitute.For<IMedicalRegistrationRepository>();
            var logger = Substitute.For<ILogger<HealthSecurityService>>();
            IHealthSecurityService service = new HealthSecurityService(repository, logger);

            repository
                .TryCreateSecurityId(default, default)
                .ReturnsForAnyArgs(true);

            string[] result = service.GenerateSecurityIds(3, 8, "comment");

            Assert.That(result, Has.Exactly(3).Items);
            Assert.That(result, Has.All.Length.EqualTo(8));

            logger.DidNotReceiveWithAnyArgs().LogError("", default);
        }

        [Test]
        public void ShouldRegenerateSecurityIdOnMatch()
        {
            var repository = Substitute.For<IMedicalRegistrationRepository>();
            var logger = Substitute.For<ILogger<HealthSecurityService>>();
            IHealthSecurityService service = new HealthSecurityService(repository, logger);

            repository
                .TryCreateSecurityId(default, default)
                .ReturnsForAnyArgs(false, false, true);

            string[] result = service.GenerateSecurityIds(1, 8, "comment");

            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result, Has.All.Length.EqualTo(8));

            logger.DidNotReceiveWithAnyArgs().LogError("", default);
            repository.ReceivedWithAnyArgs(3).TryCreateSecurityId(default, default);
        }

        [Test]
        public void ShouldFailIfExceedsAllTrials()
        {
            var repository = Substitute.For<IMedicalRegistrationRepository>();
            var logger = Substitute.For<ILogger<HealthSecurityService>>();
            IHealthSecurityService service = new HealthSecurityService(repository, logger);

            repository
                .TryCreateSecurityId(default, default)
                .ReturnsForAnyArgs(false);

            Assert.That(
                () => service.GenerateSecurityIds(1, 8, "comment"),
                Throws.InstanceOf<ApplicationException>());
            logger.ReceivedWithAnyArgs().LogError("", default);

        }
    }
}