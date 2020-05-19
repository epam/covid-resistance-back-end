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

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Epam.CovidResistance.Tools.Administrative.Cli.Services
{
    public class ConsoleUserManager<T> : UserManager<T> where T : class
    {
        private readonly CancellationTokenSource tokenSource;

        /// <summary>
        /// Constructs a new instance of <see cref="T:Microsoft.AspNetCore.Identity.UserManager`1" />.
        /// </summary>
        /// <param name="store">The persistence store the manager will operate over.</param>
        /// <param name="optionsAccessor">The accessor used to access the <see cref="T:Microsoft.AspNetCore.Identity.IdentityOptions" />.</param>
        /// <param name="passwordHasher">The password hashing implementation to use when saving passwords.</param>
        /// <param name="userValidators">A collection of <see cref="T:Microsoft.AspNetCore.Identity.IUserValidator`1" /> to validate users against.</param>
        /// <param name="passwordValidators">A collection of <see cref="T:Microsoft.AspNetCore.Identity.IPasswordValidator`1" /> to validate passwords against.</param>
        /// <param name="keyNormalizer">The <see cref="T:Microsoft.AspNetCore.Identity.ILookupNormalizer" /> to use when generating index keys for users.</param>
        /// <param name="errors">The <see cref="T:Microsoft.AspNetCore.Identity.IdentityErrorDescriber" /> used to provider error messages.</param>
        /// <param name="services">The <see cref="T:System.IServiceProvider" /> used to resolve services.</param>
        /// <param name="logger">The logger used to log messages, warnings and errors.</param>
        public ConsoleUserManager(IUserStore<T> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<T> passwordHasher, IEnumerable<IUserValidator<T>> userValidators,
            IEnumerable<IPasswordValidator<T>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<T>> logger) : base(store, optionsAccessor, passwordHasher, userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
        {
            tokenSource = new CancellationTokenSource();

            CancellationToken = tokenSource.Token;
        }

        protected override CancellationToken CancellationToken
        {
            get;
        }

        internal void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.Cancel)
            {
                tokenSource.Cancel();
            }
        }
    }
}
