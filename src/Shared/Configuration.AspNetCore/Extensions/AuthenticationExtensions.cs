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

using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddIdentityServerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection apiIdentityConfiguration = configuration.GetSection("ApiIdentity");

            return services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = apiIdentityConfiguration["Authority"];

                    options.ApiName = apiIdentityConfiguration["ClientId"];
                    options.ApiSecret = apiIdentityConfiguration["ClientSecret"];

                    options.SupportedTokens = SupportedTokens.Both;
                });
        }
    }
}