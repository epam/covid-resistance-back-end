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
using Epam.CovidResistance.Tools.Administrative.Cli.DependencyInjection;
using Epam.CovidResistance.Tools.Administrative.Cli.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Tools.Administrative.Cli.Commands.Admin
{
    [Command(Name = "create",
         Description = "Creates an user account with admin role."), HelpOption]
    internal class CreateAdminCommand
    {
        private readonly IdentityServicesAccessor identityServicesAccessor;

        public CreateAdminCommand(IdentityServicesAccessor identityServicesAccessor)
        {
            this.identityServicesAccessor = identityServicesAccessor;
        }

        [Required, Argument(0, "Name", "User name.")]
        private string Name { get; set; }

        [Required, Argument(1, "Password", "User password.")]
        private string Password { get; set; }

        [Option("--verbose", "Show verbose output.", CommandOptionType.NoValue)]
        private bool Verbose { get; set; }

        private async Task<int> OnExecute(IConsole console)
        {
            var userManager = identityServicesAccessor.GetIdentityService<ConsoleUserManager<ApplicationUser>>(Verbose);

            console.CancelKeyPress += userManager.ConsoleOnCancelKeyPress;

            var user = new ApplicationUser(Guid.NewGuid().ToString())
            {
                UserName = Name
            };

            if (!Verbose)
            {
                console.WriteLine($"Start creating new user with {Name} name.");
            }

            IdentityResult result = await userManager.CreateAsync(user, Password);

            if (!result.Succeeded)
            {
                if (!Verbose)
                {
                    console.Error.WriteLine($"Error on creating user: {FormatIdentityErrors(result.Errors)}");
                }

                return 1;
            }

            var adminRoleName = Roles.Admin.ToString("G");

            if (!Verbose)
            {
                console.WriteLine($"Successfully created user with {Name} name.");

                console.WriteLine($"Start assigning {adminRoleName} to {Name} user.");
            }

            result = await userManager.AddToRoleAsync(user, Roles.Admin.ToString("G"));

            if (!result.Succeeded)
            {
                if (!Verbose)
                {
                    console.Error.WriteLine($"Error on assigning {adminRoleName} to user {FormatIdentityErrors(result.Errors)}");
                }

                return 1;
            }

            if (!Verbose)
            {
                console.WriteLine($"Successfully assigning {adminRoleName} to {Name} user.");
            }

            return 1;
        }
        private string FormatIdentityErrors(IEnumerable<IdentityError> errors)
            => string.Join(',', errors.Select(error => error.Description));
    }
}