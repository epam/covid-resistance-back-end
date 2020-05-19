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

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Epam.CovidResistance.Services.Infection.WebApi.Entities;
using Epam.CovidResistance.Services.Infection.WebApi.Interfaces;
using Epam.CovidResistance.Services.Infection.WebApi.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.Infection.WebApi.Services
{
    /// <summary>
    /// Represent a service to save object to blob storage.
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private const string ContentType = "application/json";
        private const string UserToken = "UserToken";

        private readonly BlobServiceClient blobServiceClient;
        private readonly IOptions<BlobOptions> blobOptions;

        /// <summary>Initializes a new instance of the <see cref="BlobStorageService" /> class.</summary>
        /// <param name="blobServiceClient">The BLOB service client.</param>
        /// <param name="blobOptions">The BLOB options.</param>
        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IOptions<BlobOptions> blobOptions
        )
        {
            this.blobServiceClient = blobServiceClient;
            this.blobOptions = blobOptions;
        }

        /// <summary>Uploads the meetings to container.</summary>
        /// <param name="ownerToken">The owner token.</param>
        /// <param name="acceptRequest">The accept request.</param>
        public async Task UploadMeetingsToContainer(string ownerToken, AcceptRequest acceptRequest)
        {
            var blobName = $"{ownerToken}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            IDictionary<string, string> metadata = new Dictionary<string, string>
            {
                { UserToken, ownerToken }
            };

            await UploadBlobAsync(blobName, acceptRequest, metadata);
        }

        private async Task UploadBlobAsync(string blobName, Object obj, IDictionary<string, string> metadata = null)
        {
            BlobOptions config = blobOptions.Value;
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(config.ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            await using (var ms = new MemoryStream())
            {
                LoadStreamWithJson(obj, ms);
                await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = ContentType });
            }

            if (metadata != null && metadata.Count > 0)
            {
                await blobClient.SetMetadataAsync(metadata);
            }
        }

        private void LoadStreamWithJson(Object obj, Stream ms)
        {
            var json = JsonConvert.SerializeObject(obj);
            var writer = new StreamWriter(ms);
            writer.Write(json);
            writer.Flush();
            ms.Position = 0;
        }
    }
}
