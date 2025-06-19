// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

#nullable enable

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
        var console = this.options.Console;

        foreach (var logRecord in batch)
        {
            var severity = GetSeverityString(logRecord);
            var category = logRecord.CategoryName ?? string.Empty;
            var eventId = logRecord.EventId.Id;

            // Use FormattedMessage if available, otherwise fall back to Body
            var message = !string.IsNullOrEmpty(logRecord.FormattedMessage)
                ? logRecord.FormattedMessage
                : logRecord.Body?.ToString() ?? string.Empty;

            // Write severity in color, then rest of the line in default color
            var originalColor = console.ForegroundColor;
            console.ForegroundColor = GetSeverityColor(severity);
            console.Write(severity);
            console.ForegroundColor = originalColor;
            console.WriteLine($": {category}[{eventId}]");
            console.WriteLine($"      {message}");
        }

        return ExportResult.Success;
    }

    /// <summary>
    /// Gets the severity string for a log record.
    ///
    /// This method uses reflection to access the internal Severity property if available.
    /// When LogRecord.Severity becomes public, update this method to use it directly.
    /// </summary>
    private static string GetSeverityString(LogRecord logRecord)
    {
        // Try to get the internal Severity property via reflection
        var severityProp = logRecord.GetType().GetProperty("Severity", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (severityProp != null)
        {
            var severityValue = severityProp.GetValue(logRecord);
            if (severityValue != null)
            {
                int sevNum = (int)severityValue;

                // OpenTelemetry log severity number ranges:
                // 1-4: Trace, 5-8: Debug, 9-12: Info, 13-16: Warn, 17-20: Error, 21-24: Critical
                if (sevNum >= 1 && sevNum <= 4)
                {
                    return "trce";
                }

                if (sevNum >= 5 && sevNum <= 8)
                {
                    return "dbug";
                }

                if (sevNum >= 9 && sevNum <= 12)
                {
                    return "info";
                }

                if (sevNum >= 13 && sevNum <= 16)
                {
                    return "warn";
                }

                if (sevNum >= 17 && sevNum <= 20)
                {
                    return "fail";
                }

                if (sevNum >= 21 && sevNum <= 24)
                {
                    return "crit";
                }
            }
        }

        return "unkn";
    }

    /// <summary>
    /// Gets the console color for a severity level.
    /// </summary>
    /// <param name="severity">The severity string.</param>
    /// <returns>The console color for the severity.</returns>
    private static ConsoleColor GetSeverityColor(string severity)
    {
        return severity switch
        {
            "trce" => ConsoleColor.Gray,
            "dbug" => ConsoleColor.Cyan,
            "info" => ConsoleColor.Green,
            "warn" => ConsoleColor.Yellow,
            "fail" => ConsoleColor.Red,
            "crit" => ConsoleColor.Magenta,
            _ => ConsoleColor.White,
        };
    }
}
