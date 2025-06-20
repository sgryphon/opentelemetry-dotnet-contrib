// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;

namespace OpenTelemetry.Exporter.SimpleConsole.Tests;

internal class ThreadingTestMockConsole : IConsole
{
    private ConsoleColor foregroundColor = ConsoleColor.White;

    private ConsoleColor backgroundColor = ConsoleColor.Black;

    public ConcurrentQueue<string> Calls { get; } = new();

    public ConsoleColor ForegroundColor
    {
        get => this.foregroundColor;
        set
        {
            this.foregroundColor = value;
            this.Calls.Enqueue($"Foreground:{value}");
            Thread.Sleep(value == ConsoleColor.Yellow ? 200 : 5); // longer for warning
        }
    }

    public ConsoleColor BackgroundColor
    {
        get => this.backgroundColor;
        set
        {
            this.backgroundColor = value;
            this.Calls.Enqueue($"Background:{value}");
        }
    }

    public void ResetColor()
    {
        this.Calls.Enqueue("Reset");
    }

    public void Write(string value)
    {
        this.Calls.Enqueue($"Write:{value}");
        Thread.Sleep(value.Contains("warn", StringComparison.OrdinalIgnoreCase) ? 200 : 5);
    }

    public void WriteLine(string value)
    {
        this.Calls.Enqueue($"WriteLine:{value}");
        Thread.Sleep(value.Contains("warn", StringComparison.OrdinalIgnoreCase) ? 400 : 5);
    }
}
