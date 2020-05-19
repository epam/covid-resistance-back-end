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
using Epam.CovidResistance.Services.Infection.WebApi.Services;
using Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions;
using Epam.CovidResistance.Shared.Infrastructure.Configuration.Options;
using Epam.CovidResistance.Shared.Infrastructure.Persistence;
using Epam.CovidResistance.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Epam.CovidResistance.Services.Infection.WebApi
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


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Version = "v1.0",
                    Title = "Infection API",
                    Description = "Infection API endpoints."
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Uses full schema names to avoid v1/v2/v3 schema collisions
                // see: https://github.com/domaindrivendev/Swashbuckle/issues/442
                c.CustomSchemaIds(x => x.FullName);
            });

            services.AddAutoMapper(typeof(Startup));
            services.Configure<Metadata>(Configuration.GetSection("metadata"));
            services.Configure<Backend>(Configuration.GetSection("backend"));
            services.Configure<CassandraOptions>(Configuration.GetSection("Cassandra"));
            services.AddSingleton<ICassandraCluster, CassandraCluster>();
            services.AddSingleton<ICassandraSession, CassandraSession>();
            services.AddScoped<IStatusChangeRepository, StatusChangeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInfectionService, InfectionService>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();

            services.Configure<BlobOptions>(Configuration.GetSection("Blob"));
            services.AddAzureClients(
                builder =>
                {
                    IConfigurationSection blobConfiguration = Configuration.GetSection("Blob");
                    builder.AddBlobServiceClient(blobConfiguration["ConnectionString"]);
                });

            services.AddIdentityServerAuthentication(Configuration);
            services.AddAuthorization();
        }

        /// <summary>Configures the specified application.</summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory</param>
        /// <remarks>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</remarks>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Meeting API v1.0");
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}