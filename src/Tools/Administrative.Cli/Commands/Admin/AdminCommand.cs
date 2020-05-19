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

using McMaster.Extensions.CommandLineUtils;

namespace Epam.CovidResistance.Tools.Administrative.Cli.Commands.Admin
{
    [Command(Name = "admin",
         Description = "Manage admin accounts."), Subcommand(typeof(CreateAdminCommand)), HelpOption]
    public class AdminCommand
    {
        private int OnExecute(IConsole console)
        {
            console.Error.WriteLine("You must specify an action. See --help for more details.");

            return 1;
        }
    }
}