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

using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Epam.CovidResistance.Services.User.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids
            => new IdentityResource[]
            {
                new IdentityResources.OpenId()
            };

        public static IEnumerable<ApiResource> Apis
            => new[]
            {
                new ApiResource("userApi", "User service")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                },
                new ApiResource("infectionApi", "Infection service")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                },
                new ApiResource
                {
                    Name = "apim",
                    DisplayName = "Api management service",

                    ApiSecrets = { new Secret("secret".Sha256()) },

                    Scopes = {
                        new Scope("apim")
                        {
                            ShowInDiscoveryDocument = false
                        }
                    }
                }
            };

        public static IEnumerable<Client> Clients
            => new[]
            {
                new Client
                {
                    ClientId = "ed82e190-d95e-403e-bc8f-ca0d932b02c1",

                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    AllowedScopes = Apis.Select(apis => apis.Name).Append(IdentityServerConstants.StandardScopes.OpenId).ToList(),

                    AccessTokenType = AccessTokenType.Reference,

                    AccessTokenLifetime = (int) TimeSpan.FromMinutes(20).TotalSeconds,

                    UpdateAccessTokenClaimsOnRefresh = true,

                    RefreshTokenUsage = TokenUsage.OneTimeOnly,

                    AllowOfflineAccess = true
                }
            };
    }
}