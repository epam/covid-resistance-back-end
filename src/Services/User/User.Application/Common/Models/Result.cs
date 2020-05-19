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

using Epam.CovidResistance.Shared.Domain.Model.Errors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Epam.CovidResistance.Services.User.Application.Common.Models
{
    public readonly struct Result
    {
        internal Result(ResultStatus status, IEnumerable<InnerError> errors)
        {
            Status = status;
            Errors = errors.ToArray();
        }

        public ResultStatus Status { get; }

        public bool Succeeded => Status == ResultStatus.Succeeded;

        public InnerError[] Errors { get; }

        public static Result Success()
            => new Result(ResultStatus.Succeeded, Array.Empty<InnerError>());

        public static Result ValidationError(InnerError errors)
            => ValidationError(new[] { errors });

        public static Result ValidationError(IEnumerable<InnerError> errors)
            => new Result(ResultStatus.Validation, errors);

        public static Result FromException(Exception exception)
            => Failure(new InnerError(exception.GetType().ToString(), exception.Message));

        public static Result Failure(InnerError errors)
            => Failure(new[] { errors });

        public static Result Failure(IEnumerable<InnerError> errors)
            => new Result(ResultStatus.Critical, errors);
    }
}