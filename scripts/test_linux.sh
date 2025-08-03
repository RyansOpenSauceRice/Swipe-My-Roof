#!/bin/bash

# Fast testing script for Linux development
# This script builds and runs the Linux version of the app and runs the tests

# Exit on error
set -e

# Build the Linux app
echo "Building Linux app..."
dotnet build src/SwipeMyRoof.AvaloniaUI.Linux/SwipeMyRoof.AvaloniaUI.Linux.csproj

# Run the tests
echo "Running tests..."
dotnet test src/SwipeMyRoof.Tests/SwipeMyRoof.Tests.csproj

# If tests pass, offer to run the app
if [ $? -eq 0 ]; then
    echo "Tests passed!"
    read -p "Do you want to run the app? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "Starting the app..."
        dotnet run --project src/SwipeMyRoof.AvaloniaUI.Linux/SwipeMyRoof.AvaloniaUI.Linux.csproj
    fi
fi