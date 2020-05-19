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

namespace Epam.CovidResistance.Services.User.API.Interfaces
{
    /// <summary>
    /// The interface for health security management.
    /// </summary>
    public interface IHealthSecurityService
    {
        /// <summary>
        /// Creates health security ids.
        /// </summary>
        /// <param name="numberOfSecurityIds">The number of health security ids which need to be generated.</param>
        /// <param name="codeLength">The length of health security id.</param>
        /// <param name="comment">Comment which can be used for stating the purpose of generating health security ids.</param>
        /// <returns>Health security ids.</returns>
        string[] GenerateSecurityIds(int numberOfSecurityIds, int codeLength, string comment);
    }
}