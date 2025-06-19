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
}
