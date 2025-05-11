# Vanilla JS Frontend with .NET C# Azure Function & Cosmos DB Backend

This project demonstrates a simple web application with:

- A vanilla JavaScript frontend that makes API calls to an Azure Function
- A .NET C# Azure Function backend (using isolated process model) that connects to Cosmos DB
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

## Building the Application

### Build the Azure Functions Backend

1. Open the project in Visual Studio Code

2. Build the backend with VS Code:
   - Run the build task: Press `Ctrl+Shift+B` or run the "build" task from the Terminal menu
   - Alternatively, use the Terminal:
     ```bash
     cd backend
     dotnet build
     ```

3. The build artifacts will be created in the `/backend/bin/Debug/net8.0` directory

### Build the Frontend

The frontend uses vanilla JavaScript/HTML/CSS and doesn't require a build step.

## Running the Application

### Option 1: Using VS Code

1. Open the VS Code workspace: `az-function-app.code-workspace`

2. Start the Azure Functions backend:
   - From the Terminal menu, select "Run Task" > "build"
   - Then run the Azure Functions app:
     ```bash
     cd backend
     func start
     ```

3. Start the frontend using Live Server:
   - Right-click on `index.html` and select "Open with Live Server"
   - Or use any HTTP server:
     ```bash
     npx http-server -p 5500
     ```

4. Open your browser to `http://localhost:5500` to access the web client

### Option 2: Using the Command Line

1. Start the Azure Function:
   ```bash
   cd backend
   func start
   ```

2. Start the frontend using a web server (in a new terminal):
   ```bash
   npx http-server -p 5500   # If you have Node.js installed
   # Or use Python:
   python -m http.server 5500
   ```

3. Open your browser to `http://localhost:5500`

## Debugging the Application

### Debugging the Azure Functions Backend

The Azure Functions project uses .NET Isolated process model, which requires specific configuration for debugging.

#### Option 1: Using VS Code Debugger (Recommended)

1. Set breakpoints in your Azure Functions code (e.g., in `backend/Functions/MyFunction.cs`)

2. Start debugging:
   - Open the "Run and Debug" view in VS Code (Ctrl+Shift+D)
   - Select "Debug Azure Functions (.NET Isolated)" from the dropdown menu
   - Click the green "Start Debugging" button (or press F5)

3. Make a request to your function from the web client to hit breakpoints

#### Option 2: Using the Debugging Script

If you encounter issues with the VS Code debugger, you can use the provided debugging script:

1. Set breakpoints in your code

2. Run the debug script:
   ```bash
   ./debug-functions.sh
   ```

3. VS Code will attach to the process automatically, or you can:
   - Go to "Run and Debug" view in VS Code
   - Select "Attach to Azure Functions"
   - Select the worker process when prompted

#### Troubleshooting Breakpoints

If breakpoints aren't being hit:

1. Make sure the debugger is attached to the .NET Worker process, not just the Functions host
2. Verify debug symbols are generated (`backend/bin/Debug/net8.0/backend.pdb`)
3. Try adding `"debug_type": "portable"` to your `launch.json` configuration

### Debugging the Web Client

1. Open your browser's developer tools (F12 or right-click > Inspect)
2. Navigate to the "Sources" or "Debugger" tab
3. Set breakpoints in your JavaScript code (`app.js`)
4. Interact with the web application to trigger the breakpoints

## Using the Cosmos DB Emulator

The Azure Function is configured to use mock data by default. To use Cosmos DB:

1. Install and start the [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)

2. Initialize the database and container:
   ```bash
   cd backend
   dotnet run --project Scripts/SetupCosmosDb.csproj
   ```

3. Update `backend/local.settings.json` to disable mock data:
   ```json
   "UseMockData": "false"
   ```

4. To use a different Cosmos DB instance:
   - Update the `CosmosDbConnectionString` in `backend/local.settings.json`
   - Or set up user secrets:
     ```bash
     cd backend
     dotnet user-secrets init
     dotnet user-secrets set "CosmosDbConnectionString" "your-connection-string-here"
     ```

## Cross-Origin Resource Sharing (CORS)

The Azure Function has CORS enabled to allow requests from your web client. The configuration:

1. In `local.settings.json`:
   ```json
   "Host": {
     "CORS": "*",
     "CORSCredentials": false
   }
   ```

2. And also explicitly in the function response headers:
   ```csharp
   response.Headers.Add("Access-Control-Allow-Origin", "*");
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
