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
}
