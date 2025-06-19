// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Xunit;
using System.Linq;

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

        // Verify color changes: fg and bg for severity, then restore both
        Assert.Equal(4, mockConsole.ColorChanges.Count);
        Assert.Equal(("Foreground", ConsoleColor.DarkGreen), mockConsole.ColorChanges[0]); // Severity fg
        Assert.Equal(("Background", ConsoleColor.Black), mockConsole.ColorChanges[1]); // Severity bg
        Assert.Equal(("Foreground", ConsoleColor.White), mockConsole.ColorChanges[2]); // Restore fg
        Assert.Equal(("Background", ConsoleColor.Black), mockConsole.ColorChanges[3]); // Restore bg
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

    [Fact]
    public void TimestampOutputTest()
    {
        // Arrange
        var mockConsole = new MockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                    configure.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                    configure.UseUtcTimestamp = true;
                });
            }));

        // Act
        var logger = loggerFactory.CreateLogger<SimpleConsoleIntegrationTests>();
        logger.LogInformation("Timestamped log message");

        // Assert
        var output = mockConsole.GetOutput();
        var lines = Regex.Split(output, "\r?\n");

        // Should start with a timestamp in the given format
        Assert.Matches(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} info: ", lines[0]);
    }

    [Fact]
    public void ActivityContextOutputTest()
    {
        // Arrange
        var mockConsole = new MockConsole();
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
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
        using var activitySource = new ActivitySource("TestActivitySource");
        using var activity = activitySource.StartActivity("TestActivity");

        // Log the activity ID in the message, as in the example
        var message = $"Activity {activity?.Id} started";
        logger.LogInformation(message);

        // Assert
        var output = mockConsole.GetOutput();
        var lines = Regex.Split(output, "\r?\n");
        var activityLine = lines.FirstOrDefault(l => l.Contains("Activity"));
        Assert.NotNull(activityLine);
        Assert.Matches(@"Activity 00-[0-9a-f]{32}-[0-9a-f]{16}-00 started", activityLine);
    }
}
