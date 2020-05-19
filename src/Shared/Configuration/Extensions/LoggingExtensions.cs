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

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace Epam.CovidResistance.Shared.Configuration.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddSerilogWithInsights<T>(this IServiceCollection collection)
        {
            collection.SetupSerilog((provider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ApplicationName", typeof(T).Assembly.GetName().Name)
                    // For some reason, the Function application skips the 'Startup' extension when reference to AspNetCore.Hosting appears ... Bug in function skd ?
                    //      https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#logging-services
                    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(),
                        TelemetryConverter.Traces,
                        LogEventLevel.Information)
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                        theme: AnsiConsoleTheme.Literate);
            });

            return collection;
        }

        // Workaround from following GitHub issue on Serilog Sink. 
        //       https://github.com/serilog/serilog-sinks-applicationinsights/issues/121
        public static IServiceCollection SetupSerilog(this IServiceCollection collection, Action<IServiceProvider, LoggerConfiguration> configureLogger)
        {
            var loggerConfiguration = new LoggerConfiguration();

            configureLogger(collection.BuildServiceProvider(), loggerConfiguration);

            Logger logger = loggerConfiguration.CreateLogger();

            Log.Logger = logger;

            collection.AddSingleton<ILoggerFactory>(services =>
            {
                var factory = new SerilogLoggerFactory(null, true);

                return factory;
            });

            var diagnosticContext = new DiagnosticContext(null);

            collection.AddSingleton(diagnosticContext);

            collection.AddSingleton<IDiagnosticContext>(diagnosticContext);

            return collection;
        }
    }
}