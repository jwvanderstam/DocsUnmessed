# DocsUnmessed GUI - Build Executable Script
# This script creates a standalone Windows executable

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "DocsUnmessed GUI - Build Executable" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectPath = "src\GUI\DocsUnmessed.GUI.csproj"
$OutputDir = "dist\DocsUnmessed-GUI-Windows"
$Configuration = "Release"
$Runtime = "win-x64"

# Step 1: Verify .NET SDK
Write-Host "[1/5] Verifying .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "      Found .NET SDK: $dotnetVersion" -ForegroundColor Green

# Step 2: Clean previous builds
Write-Host "[2/5] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
    Write-Host "      Cleaned output directory" -ForegroundColor Green
}

if (Test-Path "src\GUI\bin") {
    Remove-Item "src\GUI\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "      Cleaned bin directory" -ForegroundColor Green
}

if (Test-Path "src\GUI\obj") {
    Remove-Item "src\GUI\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "      Cleaned obj directory" -ForegroundColor Green
}

# Step 3: Restore packages
Write-Host "[3/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $ProjectPath
if ($LASTEXITCODE -eq 0) {
    Write-Host "      Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "      ERROR: Package restore failed!" -ForegroundColor Red
    exit 1
}

# Step 4: Publish the application
Write-Host "[4/5] Publishing application..." -ForegroundColor Yellow
Write-Host "      Configuration: $Configuration" -ForegroundColor Gray
Write-Host "      Runtime: $Runtime" -ForegroundColor Gray
Write-Host "      Self-contained: Yes" -ForegroundColor Gray
Write-Host "      Single file: Yes" -ForegroundColor Gray

dotnet publish $ProjectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output $OutputDir `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishReadyToRun=true `
    /p:PublishTrimmed=false

if ($LASTEXITCODE -eq 0) {
    Write-Host "      Application published successfully!" -ForegroundColor Green
} else {
    Write-Host "      ERROR: Publish failed!" -ForegroundColor Red
    Write-Host "      Try opening in Visual Studio and publishing from there." -ForegroundColor Yellow
    exit 1
}

# Step 5: Verify output
Write-Host "[5/5] Verifying output..." -ForegroundColor Yellow

$exePath = Join-Path $OutputDir "DocsUnmessed.GUI.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "SUCCESS! Executable created!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Location: $exePath" -ForegroundColor Cyan
    Write-Host "Size: $sizeMB MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To run the application:" -ForegroundColor Yellow
    Write-Host "  1. Navigate to: $OutputDir" -ForegroundColor White
    Write-Host "  2. Double-click: DocsUnmessed.GUI.exe" -ForegroundColor White
    Write-Host ""
    Write-Host "To distribute:" -ForegroundColor Yellow
    Write-Host "  - Zip the entire '$OutputDir' folder" -ForegroundColor White
    Write-Host "  - Share with users (no installation needed!)" -ForegroundColor White
    Write-Host ""
    
    # List all files in output
    Write-Host "Output files:" -ForegroundColor Cyan
    Get-ChildItem $OutputDir | ForEach-Object {
        $size = if ($_.PSIsContainer) { "Folder" } else { "$([math]::Round($_.Length / 1KB, 2)) KB" }
        Write-Host "  - $($_.Name) ($size)" -ForegroundColor Gray
    }
    
} else {
    Write-Host "      ERROR: Executable not found!" -ForegroundColor Red
    Write-Host "      Expected: $exePath" -ForegroundColor Red
}

Write-Host ""
Write-Host "Build completed!" -ForegroundColor Cyan
