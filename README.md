# Vanilla JS Frontend with .NET C# Azure Function & Cosmos DB Backend

This project demonstrates a simple web application with:

- A vanilla JavaScript frontend that makes API calls to an Azure Function
- A .NET C# Azure Function backend that connects to Cosmos DB
- Local development setup with Azure Functions Core Tools and Cosmos DB Emulator

## Prerequisites

To run this project locally, you'll need:

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [Azure Functions Core Tools v4](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
3. [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
4. [Visual Studio Code](https://code.visualstudio.com/) (recommended)
5. VS Code Extensions:
   - [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
   - [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
   - [Live Server](https://marketplace.visualstudio.com/items?itemName=ritwickdey.LiveServer)

## Project Structure

```
az-function-app/
├── .devcontainer/              # Dev container configuration
├── backend/                    # Azure Function C# project
│   ├── Functions/              # Azure Function code
│   ├── Scripts/                # Setup scripts for Cosmos DB
│   └── local.settings.json     # Local settings for Azure Functions
├── index.html                  # Frontend HTML
├── app.js                      # Frontend JavaScript
├── styles.css                  # Frontend CSS
└── az-function-app.code-workspace  # VS Code workspace configuration
```

## Setup Instructions

### Option 1: Using Dev Containers (Recommended)

If you have Docker installed and VS Code with the Remote - Containers extension:

1. Open the project folder in VS Code
2. When prompted, click "Reopen in Container"
3. The dev container will install all necessary tools and dependencies

### Option 2: Manual Setup

1. Install the prerequisites listed above
2. Start the Cosmos DB Emulator
3. Initialize the Cosmos DB database and container:
   ```powershell
   cd backend
   dotnet run --project Scripts/SetupCosmosDb.csproj
   ```

## Running the Application

### Option 1: Using VS Code Tasks

1. Open the VS Code workspace: `az-function-app.code-workspace`
2. Start the Azure Functions emulator:
   - Press `Ctrl+Shift+P`, select "Tasks: Run Task", then "Start Azure Functions"
3. Start the frontend using Live Server:
   - Press `Ctrl+Shift+P`, select "Tasks: Run Task", then "Start Live Server"
4. Or start both simultaneously:
   - Press `Ctrl+Shift+P`, select "Tasks: Run Task", then "Start Development Environment"

### Option 2: Manual Commands

1. Start the Azure Function:
   ```powershell
   cd backend
   func start
   ```

2. Start the frontend (using Live Server or any HTTP server):
   ```powershell
   npx live-server --port=5500
   ```

3. Open your browser to `http://localhost:5500`

## Using the Cosmos DB Emulator

The Azure Function is configured to connect to the local Cosmos DB emulator. To use a different Cosmos DB instance:

1. Update the `CosmosDbConnectionString` in `backend/local.settings.json`
2. Or set up user secrets for the connection string:
   ```powershell
   cd backend
   dotnet user-secrets init
   dotnet user-secrets set "CosmosDbConnectionString" "your-connection-string-here"
   ```

## Deployment

### Azure Function Deployment

To deploy the Azure Function to Azure:

1. Create an Azure Function resource in the Azure Portal
2. Create an Azure Cosmos DB account
3. Deploy the function:
   ```powershell
   cd backend
   func azure functionapp publish <your-function-app-name>
   ```
4. Configure the application settings in the Azure Portal to include your production Cosmos DB connection string

### Frontend Deployment

The frontend can be deployed to Azure Static Web Apps or any static hosting service.

## Additional Resources

- [Azure Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [Azure Cosmos DB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/)
- [Working with Azure Functions in VS Code](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code)
