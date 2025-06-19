// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Text.RegularExpressions;
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
        // Arrange
        using var stringWriter = new StringWriter();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Writer = stringWriter;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger<SimpleConsoleIntegrationTests>();
        var message = "Test log message from SimpleConsole exporter";

        logger.LogInformation(message);

        // Assert
        var output = stringWriter.ToString();

        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith("info: OpenTelemetry.Exporter.SimpleConsole.Tests.SimpleConsoleIntegrationTests[0]", lines[0].Trim());
        Assert.Equal($"      {message}", lines[1].TrimEnd());
    }
}
