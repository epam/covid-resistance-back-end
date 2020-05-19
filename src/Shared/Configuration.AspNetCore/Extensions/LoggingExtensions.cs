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

using Epam.CovidResistance.Shared.Configuration.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Epam.CovidResistance.Shared.Configuration.AspNetCore.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddSerilogWithInsights<T>(this IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddLogging(builder => builder.ClearProviders());

            // Npsql is not compatible with DiagnosticSource: 
            //      https://github.com/npgsql/npgsql/issues/1893
            // Implicitly app insight will build there own Configuration with only default ASP.NET core settings files ...
            collection.AddApplicationInsightsTelemetry(configuration);

            return collection.SetupSerilog((provider, loggerConfiguration) =>
            {
                var telemetryConfig = ServiceProviderServiceExtensions.GetRequiredService<TelemetryConfiguration>(provider);

                LoggerConfigurationApplicationInsightsExtensions.ApplicationInsights(loggerConfiguration
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System", LogEventLevel.Warning)
                            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                            .Enrich.FromLogContext()
                            .Enrich.WithProperty("ApplicationName", typeof(T).Assembly.GetName().Name)
                            .WriteTo,
                        telemetryConfig,
                        TelemetryConverter.Traces,
                        LogEventLevel.Information)
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                        theme: AnsiConsoleTheme.Literate);
            });
        }
    }
}