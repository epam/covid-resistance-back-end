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
using Epam.CovidResistance.Services.User.API.Services;
using Epam.CovidResistance.Services.User.Infrastructure;
using Epam.CovidResistance.Services.User.Infrastructure.Persistence;
using Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions;
using Epam.CovidResistance.Shared.IdentityDbContext.Persistence;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Epam.CovidResistance.Services.User.API
{
    /// <summary>The startup class</summary>
    public class Startup
    {
        /// <summary>Initializes a new instance of the <see cref="Startup"/> class.</summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>Gets the configuration.</summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; }

        /// <summary>Configures the services.</summary>
        /// <param name="services">The services.</param>
        /// <remarks>This method gets called by the runtime. Use this method to add services to the container.</remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilogWithInsights<Startup>(Configuration);

            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                .ConfigureApiBehaviorOptions(options
                    => options.InvalidModelStateResponseFactory = ModelStateExtensions.InvalidModelStateResponseFactory);

            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();

            services.AddInfrastructure(Configuration);

            services.AddOptions<Metadata>()
                .Configure<IConfiguration>((settings, config) =>
                {
                    config.GetSection(nameof(Metadata).ToLower()).Bind(settings);
                });

            services.AddScoped<IUserStateService, UserStateService>();
            services.AddScoped<IHealthSecurityService, HealthSecurityService>();

            services.AddIdentityServerAuthentication(Configuration);
            services.AddAuthorization();
        }

        /// <summary>Configures the specified application.</summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        /// <remarks>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</remarks>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                ApplicationDbContextSeed.InitializeDatabase(Configuration);
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}