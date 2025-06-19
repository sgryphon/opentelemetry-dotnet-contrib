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
        // TODO: Implement export logic
        return ExportResult.Success;
    }
}
