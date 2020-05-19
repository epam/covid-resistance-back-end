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
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Epam.CovidResistance.Tools.Administrative.Cli.Configuration
{
    public class PromptConfigurationProvider : ConfigurationProvider
    {
        private readonly string[] configurationList;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptConfigurationProvider"></see> class.
        /// </summary>
        public PromptConfigurationProvider(string[] configurationList)
        {
            this.configurationList = configurationList;
        }

        public override bool TryGet(string key, out string value)
        {
            value = null;

            if (configurationList.Contains(key))
            {
                value = Prompt.GetPassword("Please enter your Identity database connection string: ");

                return !string.IsNullOrEmpty(value);
            }

            return false;
        }
    }
}