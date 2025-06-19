# SimpleConsoleExporter Example for OpenTelemetry .NET

Project structure:

* Examples.SimpleConsoleExporter

  This example demonstrates how to use the SimpleConsoleExporter for OpenTelemetry logging in a .NET Generic Host application.

  The Worker logs messages at all levels, uses structured logging, exception logging, logging with scopes, custom state, and ActivitySource tracing. All output is sent to the console using the selected logger.

## Usage

You can compare different logger outputs by passing the `--logger` flag:

- nothing / `OTEL-SIMPLECONSOLE`: OpenTelemetry SimpleConsoleExporter
- `OTEL-CONSOLE`: OpenTelemetry ConsoleExporter
- `DEFAULT` / `DOTNET`: .NET Console logger
- `DOTNET-JSON`: .NET Console logger (JSON formatter)
- `DOTNET-SYSTEMD`: .NET Console logger (Systemd/Syslog formatter, .NET 8+)

### Timestamp Output

You can add a timestamp to each log line by passing the `--timestamp` argument with a format string (e.g., `"HH:mm:ss "`). This applies to both the .NET default console and the otel simpleconsole exporter.

### Example commands

```
dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --timestamp "HH:mm:ss "

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger DEFAULT --timestamp "HH:mm:ss "

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger OTEL-CONSOLE

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger DOTNET-JSON

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger DOTNET-SYSTEMD
```
