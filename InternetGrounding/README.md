# InternetGrounding MCP Server

This project is a Model Context Protocol (MCP) server that provides a tool to interact with the Google Gemini API. It allows clients communicating via MCP to retrieving up-to-date information with the help of a search engine.

## Prerequisites

*   .NET SDK (version 9.0 or later)

## Setup

1.  Clone this repository.
2.  Navigate to the `InternetGrounding` directory.
3.  Configure your Google Gemini API key:
    *   Open the `appsettings.json` file.
    *   Replace `"YOUR_GEMINI_API_KEY"` with your actual API key from Google AI Studio or Google Cloud.
    *   The `appsettings.json` should look like this:

    ```json
    {
      "GeminiApi": {
        "GEMINI_API_KEY": "YOUR_GEMINI_API_KEY",
        "MODEL_ID": "gemini-2.5-flash",
        "GENERATE_CONTENT_API": "generateContent"
      }
    }
    ```

## Building and Running

1.  Open a terminal in the `InternetGrounding` directory.
2.  Build the project:

    ```pwsh
    dotnet build
    ```

3.  Run the server:

    ```pwsh
    dotnet run
    ```

The server will start and listen for MCP communication, typically over standard input/output.

## Usage

Once the server is running, an MCP client can connect to it and use the `AskGemini` tool provided by the server. The `AskGemini` tool takes a text prompt as input and returns the response from the configured Google Gemini model.