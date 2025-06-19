// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

#nullable enable

namespace OpenTelemetry.Exporter.SimpleConsole;

/// <summary>
/// Options for the SimpleConsoleExporter.
/// </summary>
public class SimpleConsoleExporterOptions
{
    /// <summary>
    /// Gets or sets the console to use for output. Defaults to SystemConsole.
    /// </summary>
    public IConsole Console { get; set; } = new SystemConsole();

    /// <summary>
    /// Gets or sets the timestamp format string. If null, no timestamp is output.
    /// </summary>
    public string? TimestampFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use UTC timestamps. If false, local time is used.
    /// </summary>
    public bool UseUtcTimestamp { get; set; }
}
