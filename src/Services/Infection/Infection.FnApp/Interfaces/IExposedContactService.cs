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

using Epam.CovidResistance.Services.Infection.FnApp.Entities;
using Microsoft.Azure.ServiceBus.Core;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.FnApp.Interfaces
{
    /// <summary>
    /// The ExposedContactService interface.
    /// </summary>
    public interface IExposedContactService
    {
        /// <summary>Processes the contact.</summary>
        /// <param name="exposedContact">The exposed contact.</param>
        /// <returns></returns>
        Task ProcessContact(ExposedContact exposedContact);

        /// <summary>Gets or sets the message sender.</summary>
        /// <value>The message sender.</value>
        MessageSender MessageSender { get; set; }
    }
}