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

using Epam.CovidResistance.Shared.IdentityDbContext;
using Epam.CovidResistance.Shared.IdentityDbContext.Identity;
using Epam.CovidResistance.Tools.Administrative.Cli.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Epam.CovidResistance.Tools.Administrative.Cli.DependencyInjection
{
    internal class IdentityServicesAccessor : IDisposable
    {
        private readonly IConfiguration configuration;
        private IServiceScope scope;
        private ServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServicesAccessor"></see> class.
        /// </summary>
        public IdentityServicesAccessor(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            scope?.Dispose();
            serviceProvider?.Dispose();
        }

        public T GetIdentityService<T>()
            => GetIdentityService<T>(false);

        public T GetIdentityService<T>(bool verbose)
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                if (verbose)
                {
                    builder.AddFilter("Microsoft", LogLevel.Information);
                    builder.AddConsole();
                }
            });
            services.AddIdentityWithStore(configuration);
            services.AddScoped<ConsoleUserManager<ApplicationUser>>();

            serviceProvider = services.BuildServiceProvider();
            scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            return scope.ServiceProvider.GetRequiredService<T>();
        }
    }
}