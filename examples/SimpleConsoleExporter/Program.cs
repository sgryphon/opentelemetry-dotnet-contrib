// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace Examples.SimpleConsoleExporter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(TimeProvider.System);
                services.AddHostedService<Worker>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.AddSimpleConsoleExporter();
                });
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .Build();

        await host.RunAsync();
    }
}
