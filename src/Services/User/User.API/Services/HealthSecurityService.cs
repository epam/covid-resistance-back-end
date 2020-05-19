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

using Epam.CovidResistance.Services.User.API.Interfaces;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Epam.CovidResistance.Services.User.API.Services
{
    /// <summary>
    /// The service for health security management.
    /// </summary>
    public class HealthSecurityService : IHealthSecurityService
    {
        private readonly char[] dictionary = "2345679ACDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private readonly ILogger<HealthSecurityService> logger;
        private readonly IMedicalRegistrationRepository medicalRegistrationRepository;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthSecurityService"/> class.
        /// </summary>
        public HealthSecurityService(
            IMedicalRegistrationRepository medicalRegistrationRepository,
            ILogger<HealthSecurityService> logger
        )
        {
            this.medicalRegistrationRepository = medicalRegistrationRepository;
            this.logger = logger;
            random = new Random();
        }

        /// <summary>
        /// Creates health security ids.
        /// </summary>
        /// <param name="numberOfSecurityIds">The number of health security ids which need to be generated.</param>
        /// <param name="codeLength">The length of health security id.</param>
        /// <param name="comment">Comment which can be used for stating the purpose of generating health security ids.</param>
        /// <returns>Health security ids.</returns>
        public string[] GenerateSecurityIds(int numberOfSecurityIds, int codeLength, string comment)
        {
            return Enumerable.Range(1, numberOfSecurityIds)
                .Select(i => CreateSecurityId(codeLength, comment))
                .ToArray();
        }

        private string CreateSecurityId(int codeLength, string comment)
        {
            var trials = 100;
            string securityId;
            bool isCreated;
            do
            {
                securityId = GenerateSecurityId(codeLength);
                isCreated = medicalRegistrationRepository.TryCreateSecurityId(securityId, comment);
            } while (!isCreated && --trials > 0);

            if (!isCreated)
            {
                logger.LogError("Cannot create security id - all trials failed.");

                throw new ApplicationException("Cannot generate security ids. Try again later.");
            }

            return securityId;
        }

        private string GenerateSecurityId(int codeLength)
        {
            codeLength = Math.Max(codeLength, 0);
            char[] chars = Enumerable.Range(1, codeLength)
                .Select(i => dictionary[random.Next(0, dictionary.Length - 1)])
                .ToArray();

            return new string(chars);
        }
    }
}