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

using Epam.CovidResistance.Services.User.IdentityServer.Persistence;
using Epam.CovidResistance.Services.User.IdentityServer.Service;
using Epam.CovidResistance.Services.User.Infrastructure;
using Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions;
using Epam.CovidResistance.Shared.IdentityDbContext.Identity;
using Epam.CovidResistance.Shared.IdentityDbContext.Persistence;
using IdentityServer4.AspNetIdentity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Epam.CovidResistance.Services.User.IdentityServer
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilogWithInsights<Startup>(Configuration);

            services.AddInfrastructure(Configuration);

            IHealthChecksBuilder healthCheckBuilder = services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();

            IIdentityServerBuilder builder = services.AddIdentityServer()
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<RolesProfileService>()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator<ApplicationUser>>();

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();

                builder.AddInMemoryIdentityResources(Config.Ids)
                    .AddInMemoryApiResources(Config.Apis)
                    .AddInMemoryClients(Config.Clients);
            }
            else
            {
                healthCheckBuilder
                    .AddDbContextCheck<ISConfigurationDbContext>()
                    .AddDbContextCheck<ISPersistedGrantDbContext>();

                builder.AddSigningCredential(GetCertificateAsync(Configuration).Result);

                var identityServerConnectionString = Configuration.GetConnectionString("IdentityServerConnectionString");

                builder
                    .AddConfigurationStore<ISConfigurationDbContext>(options =>
                    {
                        options.ConfigureDbContext = configurationDbBuilder =>
                            configurationDbBuilder.UseNpgsql(identityServerConnectionString);
                    })
                    .AddOperationalStore<ISPersistedGrantDbContext>(options =>
                    {
                        options.ConfigureDbContext = operationalDbBuilder =>
                            operationalDbBuilder.UseNpgsql(identityServerConnectionString);

                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = (int)TimeSpan.FromDays(1).TotalSeconds;
                    });
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                IdentityServerSeed.InitializeDatabase(app);
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
        }

        private static async Task<X509Certificate2> GetCertificateAsync(IConfiguration configuration)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            IConfigurationSection keyVaultConfiguration = configuration.GetSection("KeyVault");

            SecretBundle rawSecret =
                await keyVaultClient.GetSecretAsync(keyVaultConfiguration["BaseUrl"], keyVaultConfiguration["SecretName"]);

            return new X509Certificate2(Convert.FromBase64String(rawSecret.Value),
                (string)null,
                X509KeyStorageFlags.MachineKeySet);
        }
    }
}