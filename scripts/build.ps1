#!/usr/bin/env pwsh
# Build script for DocsUnmessed

Write-Host "Building DocsUnmessed..." -ForegroundColor Cyan

# Restore dependencies
Write-Host "`nRestoring dependencies..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore dependencies" -ForegroundColor Red
    exit 1
}

# Build project
Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
