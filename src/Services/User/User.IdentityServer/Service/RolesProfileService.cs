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
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.User.IdentityServer.Service
{
    public class RolesProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory;
        private readonly UserManager<ApplicationUser> userManager;

        public RolesProfileService(IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            AspNetUserManager<ApplicationUser> userManager)
        {
            this.claimsFactory = claimsFactory;
            this.userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            ApplicationUser user = await userManager.FindByIdAsync(sub);
            if (user == null)
            {
                throw new ArgumentException($"Something in {nameof(RolesProfileService)}");
            }

            ClaimsPrincipal principal = await claimsFactory.CreateAsync(user);
            List<Claim> claims = principal.Claims.ToList();

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            ApplicationUser user = await userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}