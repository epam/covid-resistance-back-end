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
using Epam.CovidResistance.Services.Infection.FnApp.Entities;
using Epam.CovidResistance.Shared.Domain.Model;

namespace Epam.CovidResistance.Services.Infection.FnApp.Mapper
{
    /// <summary>
    /// The mapping profile class
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>Initializes a new instance of the <see cref="MappingProfile" /> class.</summary>
        public MappingProfile()
        {
            CreateMap<Meeting, MeetingContact>()
                .ForCtorParam("meetingTime", x => x.MapFrom(y => y.Timestamp))
                .ForCtorParam("userToken", x => x.MapFrom(y => y.UserToken));

            CreateMap<MeetingContact, Meeting>()
                .ForMember(x => x.Timestamp, x => x.MapFrom(y => y.MeetingTime));

            CreateMap<Meeting, ExposedContact>()
                .ForMember(x => x.MeetingTime, x => x.MapFrom(y => y.Timestamp));
        }
    }
}
