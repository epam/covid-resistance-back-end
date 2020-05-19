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
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence
{
    public interface ICassandraCluster : IDisposable
    {
        Cluster Cluster { get; }
    }

    public class CassandraCluster : ICassandraCluster
    {
        public Cluster Cluster { get; }

        public CassandraCluster(IOptions<CassandraOptions> options)
        {
            var config = options.Value;
            var clusterBuilder = Cluster.Builder()
                .AddContactPoints(config.ContactPoints)
                .WithDefaultKeyspace(config.Keyspace);

            if (config.Credentials != null)
            {
                clusterBuilder.WithCredentials(config.Credentials.UserName, config.Credentials.Password);
            }

            if (config.Port.HasValue)
            {
                clusterBuilder.WithPort(config.Port.Value);
            }

            if (config.Ssl != null && Enum.TryParse<SslProtocols>(config.Ssl.Protocol, out var sslProtocol))
            {
                var checkCertificationRevocation =
                    config.Ssl.CheckCertificateRevocation.GetValueOrDefault();
                var sslOptions = new SSLOptions(sslProtocol, checkCertificationRevocation, ValidateServerCertificate);
                sslOptions.SetHostNameResolver((ipAddress) => config.ContactPoints.FirstOrDefault());
                clusterBuilder.WithSSL(sslOptions);
            }

            Cluster = clusterBuilder.Build();
            CreateTablesIfNeeded();
        }

        static CassandraCluster()
        {
            MappingConfiguration.Global.Define<UserStateMappings>();
            MappingConfiguration.Global.Define<UserStateHistoryMappings>();
            MappingConfiguration.Global.Define<MeetingMappings>();
            MappingConfiguration.Global.Define<StatusChangeMappings>();
            MappingConfiguration.Global.Define<MedicalRegistrationMappings>();
        }

        private void CreateTablesIfNeeded()
        {
            using ISession session = Cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            new Table<UserState>(session).CreateIfNotExists();
            new Table<UserStateHistory>(session).CreateIfNotExists();
            new Table<Meeting>(session).CreateIfNotExists();
            new Table<StatusChange>(session).CreateIfNotExists();
            new Table<MedicalRegistration>(session).CreateIfNotExists();
        }

        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }


        public void Dispose()
        {
            Cluster?.Dispose();
        }
    }
}