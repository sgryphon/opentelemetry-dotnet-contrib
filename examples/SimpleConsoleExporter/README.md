# SimpleConsoleExporter Example for OpenTelemetry .NET

Project structure:

* Examples.SimpleConsoleExporter

  This example demonstrates how to use the SimpleConsoleExporter for OpenTelemetry logging in a .NET Generic Host application.

  The Worker logs messages at all levels, uses structured logging, exception logging, logging with scopes, custom state, and ActivitySource tracing. All output is sent to the console using the SimpleConsoleExporter.

  To run the example:

  ```sh
  dotnet run --project examples/SimpleConsoleExporter/Examples.SimpleConsoleExporter.csproj
  ```
