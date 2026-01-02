#!/bin/bash
# Build script for DocsUnmessed

echo -e "\033[36mBuilding DocsUnmessed...\033[0m"

# Restore dependencies
echo -e "\n\033[33mRestoring dependencies...\033[0m"
dotnet restore

if [ $? -ne 0 ]; then
    echo -e "\033[31mFailed to restore dependencies\033[0m"
    exit 1
fi

# Build project
echo -e "\n\033[33mBuilding project...\033[0m"
dotnet build --configuration Release --no-restore

if [ $? -ne 0 ]; then
    echo -e "\033[31mBuild failed\033[0m"
    exit 1
fi

echo -e "\n\033[32mBuild completed successfully!\033[0m"
