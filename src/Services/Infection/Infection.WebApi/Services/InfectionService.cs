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

using AutoMapper;
using Epam.CovidResistance.Services.Infection.WebApi.Entities;
using Epam.CovidResistance.Services.Infection.WebApi.Interfaces;
using Epam.CovidResistance.Services.Infection.WebApi.Models;
using Epam.CovidResistance.Shared.Domain.Model;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Entities;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.WebApi.Services
{
    /// <summary>
    /// The infection service.
    /// </summary>
    public class InfectionService : IInfectionService
    {
        private readonly ILogger<InfectionService> logger;
        private readonly IMapper mapper;
        private readonly IStatusChangeRepository statusChangeRepository;
        private readonly IUserRepository userRepository;
        private readonly IBlobStorageService blobStorageService;

        private readonly int expirationPeriodInHours;
        private readonly IDictionary<int, bool> contactDiscoveryDictionary;

        /// <summary>Initializes a new instance of the <see cref="InfectionService" /> class.</summary>
        /// <param name="mapper"></param>
        /// <param name="statusChangeRepository"></param>
        /// <param name="userRepository"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="options"></param>
        /// <param name="backendOptions"></param>
        /// <param name="logger"></param>
        public InfectionService(
            IMapper mapper,
            IStatusChangeRepository statusChangeRepository,
            IUserRepository userRepository,
            IBlobStorageService blobStorageService,
            IOptions<Metadata> options,
            IOptions<Backend> backendOptions,
            ILogger<InfectionService> logger)
        {
            this.mapper = mapper;
            this.statusChangeRepository = statusChangeRepository;
            this.userRepository = userRepository;
            this.blobStorageService = blobStorageService;
            this.logger = logger;

            expirationPeriodInHours = backendOptions.Value.MedicalCode.ExpirationHours;
            contactDiscoveryDictionary = options.Value.Statuses.Values
                                             .ToDictionary(v => v.Id, v => v.RequiresContactDiscovery);
        }

        /// <summary>Gets the medical code (OTP).</summary>
        /// <returns></returns>
        public string GetMedicalCode(bool isRequiresContactDiscovery)
        {
            // generates an 8-letter one time password.
            // the last character of the generated medical code will have a special meaning:
            //   S: Requires sharing the meeting information(Share)
            //   H: Does not require sharing the meeting information(Healthy)

            var otp = GenerateOneTimePassword(maxLength: 7);

            return otp + (isRequiresContactDiscovery ? "S" : "H");
        }

        /// <summary>Initializes the status change.</summary>
        /// <param name="ownerToken">The owner token.</param>
        /// <param name="changeRequest">The change request.</param>
        /// <param name="changeResponse"></param>
        /// <returns></returns>
        public OperationResult InitStatusChange(string ownerToken, ChangeRequest changeRequest, out ChangeResponse changeResponse)
        {
            changeResponse = new ChangeResponse();
            string errorMessage;

            // Validate passed status
            if (changeRequest?.StatusId == null || !contactDiscoveryDictionary.ContainsKey(changeRequest.StatusId.Value))
            {
                errorMessage = "Invalid status Id.";
                logger.LogError(errorMessage);

                return OperationResult.CreateInstance(StatusCodes.Status500InternalServerError, errorMessage);
            }

            // Generate new code and check it in DB
            string medicalCode;
            var attemptCount = 0;
            var maxAttemptCount = 10;
            bool isInserted;
            do
            {
                medicalCode = GetMedicalCode(contactDiscoveryDictionary[changeRequest.StatusId.Value]);
                var changeRequestDto = mapper.Map<StatusChangeRequest>(changeRequest);
                isInserted = statusChangeRepository.TryCreateChangeRequest(medicalCode, changeRequestDto, ownerToken);
                attemptCount++;
            }
            while (attemptCount < maxAttemptCount && !isInserted);

            if (string.IsNullOrEmpty(medicalCode))
            {
                errorMessage = "Medical code has not been generated after all attempts.";
                logger.LogError(errorMessage);

                return OperationResult.CreateInstance(StatusCodes.Status500InternalServerError, errorMessage);
            }

            if (isInserted)
            {
                changeResponse.MedicalCode = medicalCode;
                changeResponse.ExpirationDate = DateTime.UtcNow.AddHours(expirationPeriodInHours);
            }

            return OperationResult.CreateInstance(StatusCodes.Status200OK);
        }

        /// <summary>Accepts the status.</summary>
        /// <param name="ownerToken"></param>
        /// <param name="acceptRequest">The accept request.</param>
        /// <returns></returns>
        public async Task<OperationResult> AcceptStatusAsync(string ownerToken, AcceptRequest acceptRequest)
        {
            AcceptChangeResponse statusChangeResponse =
                statusChangeRepository.TryAcceptChangeRequest(acceptRequest.MedicalCode, expirationPeriodInHours);

            switch (statusChangeResponse.Result)
            {
                case AcceptChangeResult.MissingCode:
                case AcceptChangeResult.ReusedCode:
                    return OperationResult.CreateInstance(StatusCodes.Status404NotFound, "Medical code not found or already taken.");
                case AcceptChangeResult.ExpiredCode:
                    return OperationResult.CreateInstance(StatusCodes.Status406NotAcceptable, "Medical code is expired.");
            }

            // Try upload all meetings to blob
            await blobStorageService.UploadMeetingsToContainer(ownerToken, acceptRequest);

            // Mark the change request as accepted
            var userStatus = new UserStateStatus
            {
                UserToken = ownerToken,
                StatusId = statusChangeResponse.StatusId,
                StatusChangedOn = statusChangeResponse.StatusChangedOn
            };

            // Update user status
            userRepository.SetUserStatus(userStatus);

            return OperationResult.CreateInstance(StatusCodes.Status200OK);
        }

        /// <summary>Generates the one time password.</summary>
        /// <returns></returns>
        private string GenerateOneTimePassword(int maxLength)
        {
            // Only uppercase letters and numbers. In order to avoid confusion, don’t include the letters B, O, I and the numbers 8, 0, 1.

            // Declare a string variable which stores all chars
            var baseString = "2345679ACDEFGHJKLMNPQRSTUVWXYZ";
            var chars = new char[maxLength];
            var length = baseString.Length;

            var rnd = new Random();
            for (var i = 0; i < maxLength; i++)
            {
                chars[i] = baseString[Convert.ToInt32((length - 1) * rnd.NextDouble())];
            }

            return new string(chars);
        }
    }
}