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

using Epam.CovidResistance.Shared.IdentityDbContext.Identity;
using Epam.CovidResistance.Shared.IdentityDbContext.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Epam.CovidResistance.Shared.IdentityDbContext
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityWithStore(this IServiceCollection services, IConfiguration configuration)
            => AddIdentityWithStore(services, configuration, null);

        public static IServiceCollection AddIdentityWithStore(this IServiceCollection services,
            IConfiguration configuration,
            Assembly migrationsAssembly)
        {
            services.AddDbContext<ApplicationDbContext>(builder =>
                builder.UseNpgsql(configuration.GetConnectionString("IdentityConnectionString"),
                    optionsBuilder =>
                    {
                        if (migrationsAssembly != null)
                        {
                            optionsBuilder.MigrationsAssembly(migrationsAssembly.FullName);
                        }
                    }));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}