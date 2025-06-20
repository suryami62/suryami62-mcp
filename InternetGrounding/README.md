# InternetGrounding MCP Server

A Model Context Protocol (MCP) server for retrieving up-to-date information with the help of a search engine.

## Prerequisites

- .NET SDK 9.0+

## Setup

1. Clone the repository and navigate to the `InternetGrounding` folder.
2. Configure your Gemini API key and Model ID via:
   - Command line arguments:
     ```pwsh
     dotnet run -- --GeminiApi:GEMINI_API_KEY="API_KEY" --GeminiApi:MODEL_ID="model-id"
     ```
   - Or environment variables:
     ```pwsh
     $env:GeminiApi__GEMINI_API_KEY="API_KEY"
     $env:GeminiApi__MODEL_ID="model-id"
     dotnet run
     ```

## Build & Run

```pwsh
dotnet build
dotnet run -- --GeminiApi:GEMINI_API_KEY="API_KEY" --GeminiApi:MODEL_ID="model-id"
```

## MCP Client Configuration

Example configuration in the client's `settings.json`:

```json
{
  "mcpServers": {
    "InternetGrounding": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "PATH/InternetGrounding.csproj",
        "--",
        "--GeminiApi:GEMINI_API_KEY=API_KEY",
        "--GeminiApi:MODEL_ID=model-id"
      ]
    }
  }
}
```

## Usage

Use the `AskGemini` tool to send prompts and receive responses from the Gemini API.
