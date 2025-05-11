# Azure Functions with JavaScript Frontend

A full-stack web application with a JavaScript frontend and .NET Azure Function backend, optimized for development in VS Code with Dev Containers.

![Frontend Screenshot](front-end.png)

## Dev Container Quick Start (Recommended)

1. Prerequisites:
   - [VS Code](https://code.visualstudio.com/)
   - [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
   - [Docker Desktop](https://www.docker.com/products/docker-desktop/)

2. Open in VS Code with Dev Container:
   - Clone this repository
   - Open in VS Code
   - When prompted, click "Reopen in Container"
   - 🔄 Wait for container to build (all dependencies pre-installed!)

3. Build the backend: `Ctrl+Shift+B`

4. Start the Azure Function:
   - ▶️ Press `F5` to start with debugging (recommended)
   - This launches the Function with full debugging capabilities
   - OR run in terminal:
     ```bash
     cd backend && func start
     ```

5. Launch the frontend:
   - 🔍 Right-click `index.html` → Open with Live Server
   - 🌐 App opens at http://localhost:5500

## Dev Container Debugging Experience

The Dev Container is pre-configured for an optimal debugging experience:

1. Set breakpoints in `backend/Functions/MyFunction.cs`
2. Press `F5` or select "Run and Debug" → "Debug Azure Functions (.NET Isolated)"
   (This launches the Function app with full debugging capabilities)
3. Access the frontend to trigger your breakpoints
4. For frontend debugging, use browser Developer Tools (F12)


## Key Project Files (Pre-Configured in Dev Container)

- `backend/Functions/MyFunction.cs` - Main function implementation
- `app.js` - Frontend JavaScript that calls the function
- `.vscode/launch.json` - VS Code debugging configuration (pre-configured)
- `.devcontainer/` - Dev container configuration for consistent environments

## Working in the Dev Container

The included Dev Container provides:

- 🛠️ Pre-installed tools: .NET SDK, Azure Functions Core Tools, Node.js
- 📦 VS Code extensions: Azure Functions, C#, Live Server
- 🔧 Preconfigured debugging setup for .NET isolated process functions
- 🧪 Ready-to-use environment with consistent tooling for all developers

## Alternative Setup Method

If you can't use Dev Containers, run the appropriate script:
- Linux/macOS: `./setup-and-run.sh`
- Windows: `.\Setup-And-Run.ps1`

## Configuration in the Dev Container

The Dev Container comes with all configurations pre-set for immediate development:

### CORS (Pre-configured)

Already set up in two places:
- `backend/local.settings.json` (automatically loaded):
  ```json
  "Host": { "CORS": "*" }
  ```
- Function response headers in code:
  ```csharp
  response.Headers.Add("Access-Control-Allow-Origin", "*");
  ```

### Using Real Data (Optional)

By default, the function uses mock data for immediate development. To use Cosmos DB:

1. Open `backend/local.settings.json` in VS Code (already mounted in the container)
2. Update settings:
   ```json
   "UseMockData": "false",
   "CosmosDbConnectionString": "your-connection-string"
   ```

## Dev Container Troubleshooting

### Breakpoints Not Working

- Always start the function with `F5` using "Debug Azure Functions (.NET Isolated)" configuration
- Avoid using terminal commands (`func start`) when debugging is needed
- If breakpoints aren't hitting after using `F5`:
  - Try rebuilding with `Ctrl+Shift+B`
  - Check the Debug Console for any errors

### CORS Errors

- Verify Azure Function is running on port 7124 (automatically forwarded in Dev Container)
- The Dev Container is pre-configured with correct CORS settings
- If needed, check settings in `backend/local.settings.json` (mounted from host)

### Dev Container Issues

- Try rebuilding: Command Palette (`Ctrl+Shift+P`) → Dev Containers: Rebuild Container
- Check Docker is running on your host machine
- Ensure ports 7124 and 5500 are available on your host

## Learning Resources

- [Dev Containers Tutorial](https://code.visualstudio.com/docs/devcontainers/tutorial)
- [Azure Functions in VS Code](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code)
- [.NET Isolated Process Guide](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
