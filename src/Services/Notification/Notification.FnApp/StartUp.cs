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

using Epam.CovidResistance.Services.Notification.FnApp;
using Epam.CovidResistance.Services.Notification.FnApp.Interfaces;
using Epam.CovidResistance.Services.Notification.FnApp.Options;
using Epam.CovidResistance.Services.Notification.FnApp.Services;
using Epam.CovidResistance.Shared.Configuration.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Epam.CovidResistance.Services.Notification.FnApp
{
    internal class Startup : FunctionsStartup
    {
        public IConfiguration Configuration { get; set; }

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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilogWithInsights<Startup>();

            services.Configure<NotificationHubOptions>(Configuration.GetSection("NotificationHub"));
            services.Configure<RetryPolicyOptions>(Configuration.GetSection("RetryPolicy"));

            services.AddScoped<INotificationsService, NotificationsService>();
        }
    }
}