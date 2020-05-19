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
using Epam.CovidResistance.Services.Infection.FnApp;
using Epam.CovidResistance.Services.Infection.FnApp.Interfaces;
using Epam.CovidResistance.Services.Infection.FnApp.Options;
using Epam.CovidResistance.Services.Infection.FnApp.Services;
using Epam.CovidResistance.Shared.Configuration.Extensions;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using Epam.CovidResistance.Shared.Infrastructure.Persistence;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Epam.CovidResistance.Services.Infection.FnApp
{
    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>Gets or sets the configuration.</summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; set; }

        /// <summary>Configures the specified host builder.</summary>
        /// <param name="hostBuilder">The host builder.</param>
        public override void Configure(IFunctionsHostBuilder hostBuilder)
        {
            ServiceProvider serviceProvider = hostBuilder.Services.BuildServiceProvider();
            var configurationRoot = serviceProvider.GetService<IConfiguration>();
            var configurationBuilder = new ConfigurationBuilder();
            if (configurationRoot is IConfigurationRoot)
            {
                configurationBuilder.AddConfiguration(configurationRoot);
            }

            configurationBuilder.AddAzureAppConfiguration();

            Configuration = configurationBuilder.Build();

            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));

            ConfigureServices(hostBuilder.Services);
        }

        /// <summary>Configures the services.</summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilogWithInsights<Startup>();

            services.Configure<CassandraOptions>(Configuration.GetSection("Cassandra"));
            services.Configure<Metadata>(Configuration.GetSection("metadata"));
            services.Configure<Backend>(Configuration.GetSection("backend"));
            services.Configure<RetryPolicyOptions>(Configuration.GetSection("RetryPolicy"));

            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton<ICassandraCluster, CassandraCluster>();
            services.AddSingleton<ICassandraSession, CassandraSession>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IContactTracingService, ContactTracingService>();
            services.AddScoped<IExposedContactService, ExposedContactService>();
        }
    }
}