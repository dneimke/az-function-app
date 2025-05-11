
# GitHub Copilot Prompt: Vanilla JS Frontend with .NET C# Azure Function & Cosmos DB Backend

Create a simple HTML page with a button labeled 'Call Function'. When clicked, it should trigger a JavaScript function that fetches data from `/api/myfunction` and displays the result in a div with the ID 'result'.

## Project Requirements

The Azure Function backend should:

- Be written in .NET (C#)
- Read data from a Cosmos DB database

## Implementation Instructions

# Base instructions

- Create the full MVP, don't stop until it's done! Don't wait for my feedback, don't ask whether to continue.
- Create it in this current folder, not a new folder

### Frontend (HTML & JavaScript)

1. Basic HTML structure with the button and result display area
2. JavaScript function to:
   - Handle the button click
   - Make a fetch request to the local Azure Function endpoint (`/api/myfunction`)
   - Display the response (or an error message) in the 'result' div

### Backend (Azure Function & Cosmos DB)

1. Setting up a new .NET C# Azure Function project (e.g., using the isolated worker model)
2. Creating an HTTP-triggered Azure Function named `myfunction`
3. Installing the necessary NuGet packages:
   - Microsoft.Azure.Cosmos
   - Microsoft.Azure.Functions.Worker.Extensions.Http
   - Microsoft.Azure.Functions.Worker.Sdk
   - Microsoft.Extensions.Configuration.UserSecrets (for local secrets)
4. Writing the Azure Function code in C# to:
   - Connect to Cosmos DB (demonstrating best practices for client instantiation and configuration)
   - Query data from a specified database and container (you can suggest example names like MyDatabase and MyContainer)
   - Return the queried data (or a sample/mock data if the query is complex to set up initially)

### Local Development Environment Setup

#### Azure Functions Core Tools

- Instructions for installing and using them to run the function locally (ensure commands are appropriate for .NET projects)

#### local.settings.json

- How to create this file for local development settings
- What basic settings are needed (e.g., AzureWebJobsStorage if using storage emulator or for certain bindings)
- How to add the Cosmos DB connection string (e.g., CosmosDbConnectionString)
- Recommend using User Secrets for the connection string in C# projects as a good practice, with local.settings.json as an alternative or for non-sensitive settings

#### Cosmos DB Emulator

- Instructions on how to download, install, and run the Cosmos DB emulator
- How to get the connection string from the emulator to use in local.settings.json or user secrets
- (Optional but helpful) A simple way to seed some initial data into the emulator for testing

#### Dev Containers (VS Code)

- How to set up a Dev Container for the project
- A basic devcontainer.json configuration that includes the .NET SDK, Azure Functions Core Tools, and the C# Dev Kit extension for VS Code
- Mention how this helps in standardizing the development environment

#### VS Code Project Structure & Workspace

- Suggest a simple project folder structure (e.g., a root folder with frontend and backend subfolders, where backend contains the C# project)
- Instructions on how to create and configure a VS Code Workspace (.code-workspace file) to easily manage both the frontend and backend projects together

### Example local.settings.json structure

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true", // Or point to a real storage account if needed for other bindings
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated", // Or "dotnet" for in-process model
    "CosmosDbConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" // Example emulator key - recommend moving to User Secrets for actual development
  }
}
```

> **Note**: For C# projects, connection strings and other secrets are often managed with User Secrets during local development. local.settings.json is still used by the Azure Functions runtime locally, especially for application settings that aren't secrets or for overriding configurations.

## Goal

Enable a developer to quickly bootstrap this project, run both the frontend and the .NET C# Azure Function backend locally, and have the frontend successfully call the backend which in turn interacts with the local Cosmos DB emulator.
