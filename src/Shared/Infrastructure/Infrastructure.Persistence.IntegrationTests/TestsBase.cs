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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence.IntegrationTests
{
    [SetUpFixture]
    public class TestsBase
    {
        protected ICassandraSession CassandraSession;
        protected ServiceProvider ServiceProvider;
        private ICassandraCluster cassandraCluster;

        [OneTimeSetUp]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            serviceCollection.Configure<CassandraOptions>(configuration.GetSection("Cassandra"));
            serviceCollection.AddSingleton<ICassandraCluster, CassandraCluster>();
            serviceCollection.AddSingleton<ICassandraSession, CassandraSession>();
            serviceCollection.AddSingleton<IMedicalRegistrationRepository, MedicalRegistrationRepository>();
            serviceCollection.AddSingleton<IMeetingRepository, MeetingRepository>();
            serviceCollection.AddSingleton<IStatusChangeRepository, StatusChangeRepository>();
            serviceCollection.AddSingleton<IUserRepository, UserRepository>();
            ServiceProvider = serviceCollection.BuildServiceProvider();

            CassandraSession = ServiceProvider.GetService<ICassandraSession>();
            cassandraCluster = ServiceProvider.GetService<ICassandraCluster>();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            var options = ServiceProvider.GetService<IOptions<CassandraOptions>>();
            CassandraSession.Session.DeleteKeyspaceIfExists(options.Value.Keyspace);
            CassandraSession.Dispose();
            cassandraCluster.Dispose();
        }
    }
}
