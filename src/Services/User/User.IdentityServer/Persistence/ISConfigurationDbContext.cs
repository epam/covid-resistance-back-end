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

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace Epam.CovidResistance.Services.User.IdentityServer.Persistence
{
    public class ISConfigurationDbContext : ConfigurationDbContext<ISConfigurationDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="storeOptions">The store options.</param>
        /// <exception cref="T:System.ArgumentNullException">storeOptions</exception>
        public ISConfigurationDbContext(DbContextOptions<ISConfigurationDbContext> options,
            ConfigurationStoreOptions storeOptions)
            : base(options, storeOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("configuration");
        }
    }
}