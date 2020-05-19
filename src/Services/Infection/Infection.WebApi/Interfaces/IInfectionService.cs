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

using Epam.CovidResistance.Services.Infection.WebApi.Entities;
using Epam.CovidResistance.Services.Infection.WebApi.Models;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.WebApi.Interfaces
{
    /// <summary>
    /// The InfectionService interface.
    /// </summary>
    public interface IInfectionService
    {
        /// <summary>
        /// Initialize a status changing.
        /// </summary>
        /// <param name="ownerToken">
        /// The owner token.
        /// </param>
        /// <param name="changeRequest">
        /// The change request.
        /// </param>
        /// <param name="changeResponse">
        /// The change response.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        OperationResult InitStatusChange(string ownerToken, ChangeRequest changeRequest, out ChangeResponse changeResponse);

        /// <summary>
        /// Accept a status changing.
        /// </summary>
        /// <param name="ownerToken">
        /// The owner token.
        /// </param>
        /// <param name="acceptRequest">
        /// The accept request.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<OperationResult> AcceptStatusAsync(string ownerToken, AcceptRequest acceptRequest);
    }
}