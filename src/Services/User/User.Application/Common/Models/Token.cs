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

using System.Text.Json.Serialization;

namespace Epam.CovidResistance.Services.User.Application.Common.Models
{
    public class Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"></see> class.
        /// </summary>
        public Token(string accessToken, string refreshToken, int expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }

        /// <summary>
        /// Gets or sets AccessToken.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets RefreshToken.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets in seconds when the access token expires.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}