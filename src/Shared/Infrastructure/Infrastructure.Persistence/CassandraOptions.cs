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

namespace Epam.CovidResistance.Shared.Infrastructure.Persistence
{
    public class CassandraOptions
    {
        public class CredentialsOptions
        {
            public string UserName { get; set; }

            public string Password { get; set; }
        }

        public class SslOptions
        {
            public string Protocol { get; set; }

            public bool? CheckCertificateRevocation { get; set; }
        }

        public string[] ContactPoints { get; set; }

        public string Keyspace { get; set; }

        public int? Port { get; set; }

        public CredentialsOptions Credentials { get; set; }

        public SslOptions Ssl { get; set; }
    }
}