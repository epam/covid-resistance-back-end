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

using IdentityModel;
using System.Security.Claims;

namespace Epam.CovidResistance.Services.User.API.Extensions
{
    /// <summary>
    /// Represents extension methods to <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the user token from claims.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> instance.</param>
        /// <returns>The user token.</returns>
        public static string GetToken(this ClaimsPrincipal user)
            => user.FindFirst(JwtClaimTypes.Subject).Value;
    }
}