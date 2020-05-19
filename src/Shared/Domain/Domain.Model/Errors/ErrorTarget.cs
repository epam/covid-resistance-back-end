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

namespace Epam.CovidResistance.Shared.Domain.Model.Errors
{
    /// <summary>
    /// Represents a class with machine-friendly error target constants which are used in error handling.
    /// </summary>
    public static class ErrorTarget
    {
        /// <summary>
        /// User is not found by user token.
        /// </summary>
        public const string UserNotFound = "UserNotFound";

        /// <summary>
        /// The user token is already taken.
        /// </summary>
        public const string DuplicateUserToken = "DuplicateUserToken";

        /// <summary>
        /// Health security id is already taken or expired.
        /// </summary>
        public const string InvalidHealthSecurityId = "InvalidHealthSecurityId";

        /// <summary>
        /// Unable retrieve access and refresh tokens.
        /// </summary>
        public const string Token = "Token";

        /// <summary>
        /// Cannot create user state or history in the database.
        /// </summary>
        public const string UserStateFailure = "UserStateFailure";

        /// <summary>
        /// Medical code has not been generated.
        /// </summary>
        public const string MedicalCodeNotGenerated = "MedicalCodeNotGenerated";

        /// <summary>
        /// Unhandled exception ocurred during processing the status change request.
        /// </summary>
        public const string StatusChangeRequestRejected = "StatusChangeRequestRejected";

        /// <summary>
        /// Medical code not found or already taken.
        /// </summary>
        public const string AcceptStatusCodeNotFound = "AcceptStatusCodeNotFound";

        /// <summary>
        /// Medical code is expired.
        /// </summary>
        public const string AcceptStatusCodeExpired = "AcceptStatusCodeExpired";

        /// <summary>
        /// Accept status request is failed.
        /// </summary>
        public const string AcceptStatusFailed = "AcceptStatusFailed";

        /// <summary>
        /// Unhandled exception ocurred during processing the accept status request.
        /// </summary>
        public const string AcceptStatusRejected = "AcceptStatusRejected";
    }
}
