#!/bin/bash

# Setup and run script for Linux environments

# Check prerequisites
check_command() {
    if command -v $1 &> /dev/null
    then
        return 0
    else
        return 1
    fi
}

show_status() {
    if [ $? -eq 0 ]; then
        echo -e "\e[32m✓ $1\e[0m"
    else
        echo -e "\e[31m✗ $1\e[0m"
    fi
}

echo -e "\e[36mChecking prerequisites...\e[0m"

# Check .NET SDK
if check_command dotnet; then
    version=$(dotnet --version)
    echo -e "\e[32m✓ .NET SDK is installed (version: $version)\e[0m"
else
    echo -e "\e[31m✗ .NET SDK is not installed. Please install .NET 8.0 or later.\e[0m"
    exit 1
fi

# Check Azure Functions Core Tools
if check_command func; then
    version=$(func --version | head -n 1)
    echo -e "\e[32m✓ Azure Functions Core Tools is installed (version: $version)\e[0m"
else
    echo -e "\e[31m✗ Azure Functions Core Tools is not installed.\e[0m"
    exit 1
fi

# Check Node.js
if check_command node; then
    version=$(node --version)
    echo -e "\e[32m✓ Node.js is installed (version: $version)\e[0m"
else
    echo -e "\e[33m⚠ Node.js is not installed. This is optional for running a simple HTTP server.\e[0m"
fi

echo -e "\e[36m\nBuilding the solution...\e[0m"

# Build the backend
cd backend
dotnet build
show_status "Backend build completed"
cd ..

echo -e "\e[36m\nStarting the development environment...\e[0m"

# Start Azure Functions in background
echo "Starting Azure Functions backend..."
cd backend
start_func() {
    func start --port 7124
}
start_func &
func_pid=$!
cd ..

echo "Azure Functions backend started with process ID: $func_pid"

# Start a simple HTTP server for the frontend
echo "Starting frontend server..."
if check_command npx; then
    npx http-server -p 5500 &
    http_pid=$!
    echo "Frontend server started with process ID: $http_pid"
    echo -e "\e[32m✓ Frontend available at: http://localhost:5500\e[0m"
elif check_command python3; then
    python3 -m http.server 5500 &
    http_pid=$!
    echo "Frontend server started with process ID: $http_pid"
    echo -e "\e[32m✓ Frontend available at: http://localhost:5500\e[0m"
else
    echo -e "\e[33m⚠ Could not start frontend server. Please manually serve the frontend files.\e[0m"
fi

echo -e "\e[36m\nApplication is running!\e[0m"
echo "- Backend API: http://localhost:7124/api/myfunction"
echo "- Frontend: http://localhost:5500"
echo ""
echo "Press Ctrl+C to stop both services"

# Handle shutdown gracefully
trap "kill $func_pid $http_pid 2>/dev/null; echo -e '\n\e[36mShutting down services...\e[0m'; exit 0" INT

# Keep the script running
wait
