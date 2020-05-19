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

using Epam.CovidResistance.Tools.Administrative.Cli.Commands;
using Epam.CovidResistance.Tools.Administrative.Cli.Configuration;
using Epam.CovidResistance.Tools.Administrative.Cli.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Tools.Administrative.Cli
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            await Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Add(new PromptConfigurationSource(new[] { "ConnectionStrings:IdentityConnectionString" }));
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<IdentityServicesAccessor>();
                })
                .RunCommandLineApplicationAsync<Resist>(args);
        }
    }
}