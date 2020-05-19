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

using Epam.CovidResistance.Services.User.Application.Common.Interfaces;
using Epam.CovidResistance.Services.User.Infrastructure.Identity;
using Epam.CovidResistance.Services.User.Infrastructure.Models;
using Epam.CovidResistance.Services.User.Infrastructure.Persistence;
using Epam.CovidResistance.Shared.IdentityDbContext;
using Epam.CovidResistance.Shared.IdentityDbContext.Identity;
using Epam.CovidResistance.Shared.Infrastructure.Persistence;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Epam.CovidResistance.Services.User.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ClientIdentityOptions>()
                .Configure<IConfiguration>((settings, config) =>
                {
                    config.GetSection("ClientIdentity").Bind(settings);
                });

            services.AddOptions<CassandraOptions>()
                .Configure<IConfiguration>((settings, config) =>
                {
                    config.GetSection("Cassandra").Bind(settings);
                });

            services.AddSingleton<ICassandraCluster, CassandraCluster>();
            services.AddSingleton<ICassandraSession, CassandraSession>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IMedicalRegistrationRepository, MedicalRegistrationRepository>();

            services.AddScoped<AspNetUserManager<ApplicationUser>>();

            services.AddTransient<IIdentityService, IdentityService>();

            services.AddIdentityWithStore(configuration, typeof(ApplicationDbContextSeed).Assembly);

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 4;
            });

            return services;
        }
    }
}