// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

#nullable enable

using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace OpenTelemetry.Exporter.SimpleConsole;

/// <summary>
/// Simple console exporter for OpenTelemetry logs.
/// </summary>
public class SimpleConsoleExporter : BaseExporter<LogRecord>
{
    private readonly SimpleConsoleExporterOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleConsoleExporter"/> class.
    /// </summary>
    /// <param name="options">The exporter options.</param>
    public SimpleConsoleExporter(SimpleConsoleExporterOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        var writer = this.options.Writer ?? Console.Out;

        foreach (var logRecord in batch)
        {
            var severity = GetSeverityString(logRecord.LogLevel);
            var category = logRecord.CategoryName ?? string.Empty;
            var eventId = logRecord.EventId.Id;

            // Use FormattedMessage if available, otherwise fall back to Body
            var message = !string.IsNullOrEmpty(logRecord.FormattedMessage)
                ? logRecord.FormattedMessage
                : logRecord.Body?.ToString() ?? string.Empty;

            writer.WriteLine($"{severity}: {category}[{eventId}]");
            writer.WriteLine($"      {message}");
        }

        return ExportResult.Success;
    }

    private static string GetSeverityString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => "info",
        };
    }
}
