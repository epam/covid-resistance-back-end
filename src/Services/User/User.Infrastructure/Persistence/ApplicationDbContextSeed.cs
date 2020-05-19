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
using System;
using System.Linq;

namespace Epam.CovidResistance.Services.User.Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static void InitializeDatabase(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddInfrastructure(configuration);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.Migrate();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in Enum.GetNames(typeof(Roles)))
            {
                IdentityRole userRole = roleMgr.FindByNameAsync(role).Result;

                if (userRole == null)
                {
                    userRole = new IdentityRole(role);

                    IdentityResult result = roleMgr.CreateAsync(userRole).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine($"{role} role created");
                }
                else
                {
                    Console.WriteLine($"{role} role already exists");
                }
            }
        }
    }
}