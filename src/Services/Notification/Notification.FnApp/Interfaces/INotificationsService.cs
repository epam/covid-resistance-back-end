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

using Epam.CovidResistance.Services.Notification.FnApp.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Notification.FnApp.Interfaces
{
    /// <summary>
    /// The NotificationsService interface.
    /// </summary>
    public interface INotificationsService
    {
        /// <summary>Pushes the message asynchronous.</summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<IActionResult> PushMessageAsync(PushNotification request);
    }
}