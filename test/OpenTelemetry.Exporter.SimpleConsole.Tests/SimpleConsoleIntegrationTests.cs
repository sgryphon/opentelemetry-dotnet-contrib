// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

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
        var mockConsole = new MockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger<SimpleConsoleIntegrationTests>();
        var message = "Test log message from SimpleConsole exporter";

#pragma warning disable CA2254 // Template should be a static string
        logger.LogInformation(message);
#pragma warning restore CA2254 // Template should be a static string

        // Assert
        var output = mockConsole.GetOutput();

        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith("info: OpenTelemetry.Exporter.SimpleConsole.Tests.SimpleConsoleIntegrationTests[0]", lines[0].Trim());
        Assert.Equal($"      {message}", lines[1].TrimEnd());

        // Verify color changes: one for severity color (Green for "info"), one for restore to default
        Assert.Equal(2, mockConsole.ColorChanges.Count);
        Assert.Equal(("Foreground", ConsoleColor.Green), mockConsole.ColorChanges[0]); // Severity color
        Assert.Equal(("Foreground", ConsoleColor.White), mockConsole.ColorChanges[1]); // Restore to default
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
        var mockConsole = new MockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger(category);
#pragma warning disable CA2254 // Template should be a static string
        logger.Log(logLevel, new EventId(eventId), message);
#pragma warning restore CA2254 // Template should be a static string

        // Assert
        var output = mockConsole.GetOutput();

        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith($"{expectedSeverity}: {category}[{eventId}]", lines[0].Trim());
        Assert.Equal($"      {message}", lines[1].TrimEnd());
    }

    [Fact]
    public void StructuredLoggingWithSemanticArgumentsTest()
    {
        // Arrange
        var mockConsole = new MockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger<SimpleConsoleIntegrationTests>();
        var userName = "Alice";
        var userId = 12345;
        logger.LogInformation("User {UserName} with ID {UserId} logged in", userName, userId);

        // Assert
        var output = mockConsole.GetOutput();

        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith("info: OpenTelemetry.Exporter.SimpleConsole.Tests.SimpleConsoleIntegrationTests[0]", lines[0].Trim());
        Assert.Equal("      User Alice with ID 12345 logged in", lines[1].TrimEnd());
    }

    [Fact]
    public void ExceptionLogIntegrationTest()
    {
        // Arrange
        var mockConsole = new MockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger<SimpleConsoleIntegrationTests>();
        var message = "This is an error with exception";
        Exception ex;
        try
        {
            throw new InvalidOperationException("Something went wrong!");
        }
        catch (Exception caught)
        {
            ex = caught;
        }

#pragma warning disable CA2254 // Template should be a static string
        logger.LogError(ex, message);
#pragma warning restore CA2254 // Template should be a static string

        // Assert
        var output = mockConsole.GetOutput();
        var lines = Regex.Split(output, "\r?\n");
        Assert.StartsWith("fail: OpenTelemetry.Exporter.SimpleConsole.Tests.SimpleConsoleIntegrationTests[0]", lines[0].Trim());
        Assert.Equal($"      {message}", lines[1].TrimEnd());
        Assert.Contains("System.InvalidOperationException: Something went wrong!", output);
        // Should contain at least one stack trace line, indented
        Assert.Contains("      at ", output);
    }
}
