// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Examples.SimpleConsoleExporter;

public class Worker(
    ILogger<Worker> logger,
    TimeProvider timeProvider,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("Examples.SimpleConsoleExporter.Worker");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await this.DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        // Short delay to that startup messages are output
        await Task.Delay(100, cancellationToken);

        logger.LogTrace("This is a trace message");
        logger.LogDebug("This is a debug message");
        logger.LogInformation("This is an info message");
        logger.LogWarning("This is a warning message");
        logger.LogError("This is an error message");
        logger.LogCritical("This is a critical message");

        // Structured logging
        logger.LogInformation("User {UserId} performed {Action} at {Time}", 42, "Login", timeProvider.GetUtcNow());

        // Exception logging
        try
        {
            this.ThrowSampleException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred");
        }

        // Logging with a scope
        using (logger.BeginScope("ScopeId: {ScopeId}", Guid.NewGuid()))
        {
            logger.LogInformation("This log is inside a scope");
        }

        // Logging with custom state (dictionary)
        var customState = new Dictionary<string, object>
        {
            ["CustomKey1"] = "CustomValue1",
            ["CustomKey2"] = 1234,
        };
        logger.Log(
            LogLevel.Information,
            new EventId(1001, "CustomStateEvent"),
            customState,
            null,
            (state, ex) => $"This log has custom state: {state["CustomKey1"]}");

        // ActivitySource tracing
        using (var activity = ActivitySource.StartActivity("SampleOperation", ActivityKind.Internal))
        {
            activity?.SetTag("sample.tag", "value");
            logger.LogInformation("Activity {ActivityId} started at {Time}", activity?.Id, timeProvider.GetUtcNow());

            // Simulate some work
            Task.Delay(100, cancellationToken).Wait(cancellationToken);
            logger.LogInformation("Activity finished");
        }

        // Short delay before stopping
        await Task.Delay(100, cancellationToken);

        // Stop the host after logging
        lifetime.StopApplication();

        return;
    }

    private void ThrowSampleException()
    {
        throw new InvalidOperationException("This is a sample exception for demonstration purposes.");
    }
}
