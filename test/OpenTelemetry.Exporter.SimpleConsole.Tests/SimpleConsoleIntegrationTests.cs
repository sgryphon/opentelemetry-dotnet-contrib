// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Xunit;

namespace OpenTelemetry.Exporter.SimpleConsole.Tests;

[Trait("CategoryName", "SimpleConsoleIntegrationTests")]
public class SimpleConsoleIntegrationTests
{
    [Fact]
    public void BasicLogIntegrationTest()
    {
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter();
            }));

        var logger = loggerFactory.CreateLogger(nameof(SimpleConsoleIntegrationTests));
        logger.LogInformation("Test log message from SimpleConsole exporter");
    }
}
