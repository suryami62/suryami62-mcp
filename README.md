# InternetGrounding MCP Server

InternetGrounding is a Model Context Protocol (MCP) server that enables fetching up-to-date information from the internet using the Gemini API and search engines. It is suitable for applications that require real-time data grounding.

## Key Features

- Integration with the Gemini API for information search and processing.
- Supports stdio-based MCP Server communication.
- `AskGemini` tool for Gemini-based prompts and responses.
- Flexible configuration via environment variables or CLI arguments.

## Installation

### Prerequisites

- .NET SDK 9.0+

### Installation Steps

1. Clone the repository and navigate to the `InternetGrounding` folder.
2. Configure the Gemini API Key and Model ID:
   - **Command-line arguments:**
     ```pwsh
     dotnet run -- --GeminiApi:GEMINI_API_KEY="API_KEY" --GeminiApi:MODEL_ID="model-id"
     ```
   - **Environment variables:**
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

Example configuration in the MCP client's `settings.json`:

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

Use the `AskGemini` tool to send prompts and receive responses from the Gemini API. Example:
1. Send a prompt through the MCP Client.
2. Receive a grounded answer from Gemini.
