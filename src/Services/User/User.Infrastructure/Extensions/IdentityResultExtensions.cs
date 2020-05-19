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

using Epam.CovidResistance.Services.User.Application.Common.Models;
using Epam.CovidResistance.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Epam.CovidResistance.Services.User.Infrastructure.Extensions
{
    public static class IdentityResultExtensions
    {
        public static ISet<string> ValidationErrors = typeof(IdentityErrorDescriber)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(methodInfo => methodInfo.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public static Result ToApplicationResult(this IdentityResult result)
        {
            if (result.Succeeded)
            {
                return Result.Success();
            }

            IEnumerable<InnerError> errors = result.Errors.Select(e => new InnerError(e.Code, e.Description)).ToArray();

            return errors.All(error => ValidationErrors.Contains(error.ErrorTarget))
                       ? Result.ValidationError(errors)
                       : Result.Failure(errors);
        }
    }
}