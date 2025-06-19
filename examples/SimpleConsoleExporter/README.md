# SimpleConsoleExporter Example for OpenTelemetry .NET

Project structure:

* Examples.SimpleConsoleExporter

  This example demonstrates how to use the SimpleConsoleExporter for OpenTelemetry logging in a .NET Generic Host application.

  The Worker logs messages at all levels, uses structured logging, exception logging, logging with scopes, custom state, and ActivitySource tracing. All output is sent to the console using the selected logger.

## Usage

You can compare different logger outputs by passing the `--logger` flag:

- nothing / `otel-simpleconsole`: OpenTelemetry SimpleConsoleExporter
- `otel-console`: OpenTelemetry ConsoleExporter
- `default` / `dotnet`: .NET Console logger
- `dotnet-json`: .NET Console logger (JSON formatter)
- `dotnet-systemd`: .NET Console logger (Syslog formatter)

### Example commands

```
dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj

# or:
dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger default

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger otel-console

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger dotnet-json

dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj -- --logger dotnet-systemd
```
