// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace OpenTelemetry.Exporter.SimpleConsole;

/// <summary>
/// Options for the SimpleConsoleExporter.
/// </summary>
public class SimpleConsoleExporterOptions
{
    /// <summary>
    /// Gets or sets the text writer to use for output. Defaults to Console.Out.
    /// </summary>
    public TextWriter Writer { get; set; } = Console.Out;
}