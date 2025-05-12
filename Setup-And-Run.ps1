# Setup-And-Run.ps1
# This script helps set up and run the Azure Function app with Cosmos DB emulator

function Check-Command {
    param (
        [string]$Command
    )
    
    if (Get-Command $Command -ErrorAction SilentlyContinue) {
        return $true
    }
    else {
        return $false
    }
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Cyan

$prerequisites = @(
    @{ Name = ".NET SDK"; Command = "dotnet"; MinVersion = "8.0.0"; VersionArg = "--version" },
    @{ Name = "Azure Functions Core Tools"; Command = "func"; MinVersion = "4.0.0"; VersionArg = "--version" },
    @{ Name = "Node.js"; Command = "node"; MinVersion = "14.0.0"; VersionArg = "--version" }
)

$missingPrereqs = $false

foreach ($prereq in $prerequisites) {
    if (Check-Command -Command $prereq.Command) {
        $version = Invoke-Expression "$($prereq.Command) $($prereq.VersionArg)" | Select-Object -First 1
        Write-Host "✓ $($prereq.Name) is installed (version: $version)" -ForegroundColor Green
    } else {
        Write-Host "✗ $($prereq.Name) is not installed" -ForegroundColor Red
        $missingPrereqs = $true
    }
}

# Check if Cosmos DB Emulator is installed
$cosmosEmulatorPath = "C:\Program Files\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe"
if (Test-Path $cosmosEmulatorPath) {
    Write-Host "✓ Azure Cosmos DB Emulator is installed" -ForegroundColor Green
} else {
    Write-Host "✗ Azure Cosmos DB Emulator is not installed" -ForegroundColor Red
    Write-Host "  Download it from: https://aka.ms/cosmosdb-emulator" -ForegroundColor Yellow
    $missingPrereqs = $true
}

if ($missingPrereqs) {
    Write-Host "`nPlease install missing prerequisites before continuing." -ForegroundColor Red
    exit 1
}

# Start Cosmos DB Emulator if not already running
$cosmosProcess = Get-Process -Name "CosmosDB.Emulator" -ErrorAction SilentlyContinue
if ($null -eq $cosmosProcess) {
    Write-Host "`nStarting Azure Cosmos DB Emulator..." -ForegroundColor Cyan
    Start-Process $cosmosEmulatorPath -ArgumentList "/NoFirewall", "/NoUI"
    
    # Wait for emulator to start
    Write-Host "Waiting for Cosmos DB Emulator to start (this may take a minute)..." -ForegroundColor Yellow
    $timeout = 60
    $started = $false
    $timer = [Diagnostics.Stopwatch]::StartNew()
    
    while ($timer.Elapsed.TotalSeconds -lt $timeout -and -not $started) {
        try {
            $response = Invoke-WebRequest -Uri "https://localhost:8081/_explorer/index.html" -UseBasicParsing -TimeoutSec 1 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                $started = $true
            }
        } catch {
            Start-Sleep -Seconds 5
        }
    }
    
    if ($started) {
        Write-Host "✓ Cosmos DB Emulator started successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Cosmos DB Emulator failed to start within timeout period" -ForegroundColor Red
        Write-Host "  You may need to start it manually from the Start Menu" -ForegroundColor Yellow
    }
} else {
    Write-Host "✓ Cosmos DB Emulator is already running" -ForegroundColor Green
}

# Set up database and container
Write-Host "`nSetting up Cosmos DB database and container..." -ForegroundColor Cyan
Set-Location -Path "$PSScriptRoot\Scripts"
dotnet build
dotnet run --project SetupCosmosDb.csproj
Set-Location -Path $PSScriptRoot

# Start Azure Functions in a new window
Write-Host "`nStarting Azure Functions..." -ForegroundColor Cyan
$functionsWindow = Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\backend' ; func start" -PassThru

# Start Live Server for frontend in a new window
Write-Host "`nStarting Live Server for frontend..." -ForegroundColor Cyan
$liveServerWindow = Start-Process pwsh -ArgumentList "-NoExit", "-Command", "npx live-server --port=5500" -PassThru

# Output success message
Write-Host "`n✓ Development environment is now running!" -ForegroundColor Green
Write-Host "  - Frontend: http://localhost:5500/" -ForegroundColor Cyan
Write-Host "  - Backend API: http://localhost:7071/api/myfunction" -ForegroundColor Cyan
Write-Host "  - Cosmos DB Emulator Data Explorer: https://localhost:8081/_explorer/index.html" -ForegroundColor Cyan
Write-Host "`nPress Ctrl+C in the respective terminal windows to stop the services when you're done." -ForegroundColor Yellow
