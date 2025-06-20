// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Xunit;

namespace OpenTelemetry.Exporter.SimpleConsole.Tests;

/// <summary>
/// Threading tests for SimpleConsoleExporter.
/// </summary>
public class ThreadingTests
{
    [Fact]
    public void SingleThread_SingleMessage_WritesCorrectly()
    {
        // Arrange
        var mockConsole = new ThreadingTestMockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                });
            }));

        var logger = loggerFactory.CreateLogger<ThreadingTests>();

        // Act
        logger.LogInformation("Test log message from threading test");

        // Assert
        var calls = mockConsole.Calls.ToArray();
        var fgIndex = Array.FindIndex(calls, c => c.StartsWith("Foreground:", StringComparison.Ordinal));
        Assert.True(fgIndex >= 0, "Should have a Foreground color call");
        Assert.True(calls.Length > fgIndex + 4, "Should have enough calls after Foreground");
        Assert.StartsWith("Write:info", calls[fgIndex + 2]);
        Assert.StartsWith("Foreground:", calls[fgIndex + 3]); // restore fg
        Assert.StartsWith("Background:", calls[fgIndex + 4]); // restore bg
    }

    [Fact]
    public async Task TwoThreads_MessagesAreNotInterleaved()
    {
        // Arrange
        var mockConsole = new ThreadingTestMockConsole();
        using var loggerFactory = LoggerFactory.Create(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.AddSimpleConsoleExporter(configure =>
                {
                    configure.Console = mockConsole;
                });
            }));

        var loggerInfo = loggerFactory.CreateLogger("ThreadingTests.LoggerInfo");
        var loggerWarn = loggerFactory.CreateLogger("ThreadingTests.LoggerWarn");
        var loggerFail = loggerFactory.CreateLogger("ThreadingTests.LoggerFail");
        var timeProvider = TimeProvider.System;

        // Planned schedule:
        // t=0, Warning message log starts, it should be written first
        // t=120, Info message logs, with a delay to make sure it happens after the Warning
        // t=220, Error message logs, with a delay
        // t=800, Warning message completes after delays for the colour, severity, and text
        // t=815, Info message completes (it only has 15ns delay, but should wait until after the warning)
        // t=830, Error message completes

        Console.WriteLine($"Start: {timeProvider.GetUtcNow():HH:mm:ss.fff}");

        var t1 = Task.Run(async () =>
        {
            await Task.Delay(220);
            loggerInfo.LogInformation("Info message second {Time:HH:mm:ss.fff}", timeProvider.GetUtcNow());
        });
        var t2 = Task.Run(async () =>
        {
            await Task.Delay(0);
            loggerWarn.LogWarning("Warning message first {Time:HH:mm:ss.fff}", timeProvider.GetUtcNow());
        });
        var t3 = Task.Run(async () =>
        {
            await Task.Delay(420);
            loggerFail.LogError("Error message third {Time:HH:mm:ss.fff}", timeProvider.GetUtcNow());
        });

        await Task.WhenAll(t1, t2, t3);

        Console.WriteLine($"End: {timeProvider.GetUtcNow():HH:mm:ss.fff}");

        // Assert
        var calls = mockConsole.Calls.ToArray();

        // Find the first info and last write
        var warnStart = Array.FindIndex(calls, c => c.Contains("foreground:Yellow", StringComparison.OrdinalIgnoreCase));
        var warnEnd = Array.FindIndex(calls, c => c.Contains("Warning message first", StringComparison.OrdinalIgnoreCase));
        var infoStart = Array.FindIndex(calls, c => c.Contains("foreground:DarkGreen", StringComparison.OrdinalIgnoreCase));
        var infoEnd = Array.FindIndex(calls, c => c.Contains("Info message second", StringComparison.OrdinalIgnoreCase));
        var failStart = Array.FindIndex(calls, c => c.Contains("foreground:Black", StringComparison.OrdinalIgnoreCase));
        var failEnd = Array.FindIndex(calls, c => c.Contains("Error message third", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine($"warn: {warnStart}-{warnEnd}, info: {infoStart}-{infoEnd}, fail: {failStart}-{failEnd}");
        Console.WriteLine(string.Join("\n", calls));

        Assert.True(infoStart >= 0, "Should have info messages");
        Assert.True(warnStart >= 0, "Should have warn messages");
        Assert.True(failStart >= 0, "Should have fail messages");

        Assert.True(warnStart < warnEnd, "Warn message should be in order");
        Assert.True(infoStart < infoEnd, "Info message should be in order");
        Assert.True(failStart < failEnd, "Fail message should be in order");

        Assert.True(warnEnd < infoStart, "Warn should end before info starts");
        Assert.True(infoEnd < failStart, "Info should end before fail starts");
    }
}
