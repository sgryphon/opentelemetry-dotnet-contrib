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

    [Theory]
    [InlineData(LogLevel.Trace, "TestApp", 1, "Trace message", "trce")]
    [InlineData(LogLevel.Debug, "MyApp.Services", 42, "Debug message", "dbug")]
    [InlineData(LogLevel.Information, "OpenTelemetry.Exporter.SimpleConsole.Tests.SimpleConsoleIntegrationTests", 0, "Info message", "info")]
    [InlineData(LogLevel.Warning, "MyApp.Controllers", 100, "Warning message", "warn")]
    [InlineData(LogLevel.Error, "MyApp.DataAccess", 500, "Error message", "fail")]
    [InlineData(LogLevel.Critical, "MyApp.Startup", 999, "Critical message", "crit")]
    public void LogLevelAndFormatTheoryTest(LogLevel logLevel, string category, int eventId, string message, string expectedSeverity)
    {
        // Arrange
        using var stringWriter = new StringWriter();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Writer = stringWriter;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger(category);
        logger.Log(logLevel, new EventId(eventId), message);

        // Assert
        var output = stringWriter.ToString();

        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith($"{expectedSeverity}: {category}[{eventId}]", lines[0].Trim());
        Assert.Equal($"      {message}", lines[1].TrimEnd());
    }

    [Fact]
    public void StructuredLoggingWithSemanticArgumentsTest()
    {
        // Arrange
        using var stringWriter = new StringWriter();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Writer = stringWriter;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger<SimpleConsoleIntegrationTests>();
        var userName = "Alice";
        var userId = 12345;
        logger.LogInformation("User {UserName} with ID {UserId} logged in", userName, userId);

        // Assert
        var output = stringWriter.ToString();

        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith("info: OpenTelemetry.Exporter.SimpleConsole.Tests.SimpleConsoleIntegrationTests[0]", lines[0].Trim());
        Assert.Equal("      User Alice with ID 12345 logged in", lines[1].TrimEnd());
    }
}
