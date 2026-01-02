# Create Test Data for DocsUnmessed Rules Engine
# This script creates a realistic test file structure for manual testing

param(
    [string]$TestRoot = "test-data",
    [switch]$Clean
)

# Clean existing test data if requested
if ($Clean -and (Test-Path $TestRoot)) {
    Write-Host "Cleaning existing test data..." -ForegroundColor Yellow
    Remove-Item -Path $TestRoot -Recurse -Force
}

# Create directory structure
Write-Host "Creating test directory structure..." -ForegroundColor Cyan
$dirs = @(
    "$TestRoot/Downloads",
    "$TestRoot/Documents/archive",
    "$TestRoot/Pictures",
    "$TestRoot/Videos"
)

foreach ($dir in $dirs) {
    New-Item -Path $dir -ItemType Directory -Force | Out-Null
    Write-Host "  Created: $dir" -ForegroundColor Green
}

# Helper function to create test files with specific ages
function New-TestFile {
    param(
        [string]$Path,
        [int]$DaysOld,
        [string]$Content = "Test file content"
    )
    
    # Create the file
    $Content | Out-File -FilePath $Path -Force
    
    # Set the modification time
    $date = (Get-Date).AddDays(-$DaysOld)
    (Get-Item $Path).LastWriteTime = $date
    (Get-Item $Path).CreationTime = $date
    
    Write-Host "  Created: $Path ($DaysOld days old)" -ForegroundColor Gray
}

Write-Host "`nCreating test files..." -ForegroundColor Cyan

# Downloads folder - PDFs and mixed content
Write-Host "Downloads folder:" -ForegroundColor Yellow
New-TestFile -Path "$TestRoot/Downloads/old-document.pdf" -DaysOld 120 -Content "%PDF-1.4 Old document"
New-TestFile -Path "$TestRoot/Downloads/recent-invoice.pdf" -DaysOld 10 -Content "%PDF-1.4 Recent invoice"
New-TestFile -Path "$TestRoot/Downloads/photo-vacation.jpg" -DaysOld 90 -Content "JPEG image data"
New-TestFile -Path "$TestRoot/Downloads/temp-file.txt" -DaysOld 1 -Content "Temporary notes"
New-TestFile -Path "$TestRoot/Downloads/installer.exe" -DaysOld 45 -Content "MZ executable"

# Documents folder - Office files
Write-Host "`nDocuments folder:" -ForegroundColor Yellow
New-TestFile -Path "$TestRoot/Documents/report-2024.docx" -DaysOld 50 -Content "Word document content"
New-TestFile -Path "$TestRoot/Documents/spreadsheet.xlsx" -DaysOld 30 -Content "Excel spreadsheet"
New-TestFile -Path "$TestRoot/Documents/notes.txt" -DaysOld 5 -Content "Recent meeting notes"
New-TestFile -Path "$TestRoot/Documents/presentation.pptx" -DaysOld 25 -Content "PowerPoint presentation"
New-TestFile -Path "$TestRoot/Documents/archive/old-report.pdf" -DaysOld 200 -Content "%PDF-1.4 Old archived report"

# Pictures folder - Images
Write-Host "`nPictures folder:" -ForegroundColor Yellow
New-TestFile -Path "$TestRoot/Pictures/IMG_001.jpg" -DaysOld 60 -Content "JPEG image 1"
New-TestFile -Path "$TestRoot/Pictures/vacation_2024.png" -DaysOld 45 -Content "PNG image"
New-TestFile -Path "$TestRoot/Pictures/screenshot.png" -DaysOld 2 -Content "PNG screenshot"
New-TestFile -Path "$TestRoot/Pictures/photo.gif" -DaysOld 100 -Content "GIF animation"

# Videos folder - Video files
Write-Host "`nVideos folder:" -ForegroundColor Yellow
New-TestFile -Path "$TestRoot/Videos/clip.mp4" -DaysOld 90 -Content "MP4 video file"
New-TestFile -Path "$TestRoot/Videos/recording.mov" -DaysOld 15 -Content "MOV video file"

# Create a README in the test data
$readme = @"
# Test Data for DocsUnmessed

This directory contains test files for manual testing of the rules engine.

Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

File Count: $(Get-ChildItem -Path $TestRoot -Recurse -File | Measure-Object | Select-Object -ExpandProperty Count)

## Structure
- Downloads/ - Mixed files, some old PDFs
- Documents/ - Office files, various ages
- Pictures/ - Image files (jpg, png, gif)
- Videos/ - Video files (mp4, mov)

## Testing
1. Run assess: dotnet run -- assess --providers fs_local --root $TestRoot --out test-scan.json
2. Run simulate: dotnet run -- simulate --scan-id <id> --rules examples/mapping-rules.json --out suggestions.json

See docs/MANUAL-TEST-PLAN.md for detailed test scenarios.
"@

$readme | Out-File -FilePath "$TestRoot/README.md" -Force

# Summary
Write-Host "`n" + ("="*60) -ForegroundColor Cyan
Write-Host "Test data created successfully!" -ForegroundColor Green
Write-Host ("="*60) -ForegroundColor Cyan

$stats = @{
    TotalFiles = (Get-ChildItem -Path $TestRoot -Recurse -File | Measure-Object).Count
    TotalSize = (Get-ChildItem -Path $TestRoot -Recurse -File | Measure-Object -Property Length -Sum).Sum
    Directories = (Get-ChildItem -Path $TestRoot -Recurse -Directory | Measure-Object).Count
}

Write-Host "`nStatistics:" -ForegroundColor Cyan
Write-Host "  Total Files: $($stats.TotalFiles)" -ForegroundColor White
Write-Host "  Total Size: $([Math]::Round($stats.TotalSize / 1KB, 2)) KB" -ForegroundColor White
Write-Host "  Directories: $($stats.Directories)" -ForegroundColor White
Write-Host "  Root: $TestRoot" -ForegroundColor White

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "  1. Review test data: Get-ChildItem -Path $TestRoot -Recurse" -ForegroundColor Yellow
Write-Host "  2. Run assessment: dotnet run -- assess --providers fs_local --root $TestRoot --out test-scan.json" -ForegroundColor Yellow
Write-Host "  3. Run simulation: dotnet run -- simulate --scan-id <id> --rules examples/mapping-rules.json --out suggestions.json" -ForegroundColor Yellow
Write-Host "  4. See docs/MANUAL-TEST-PLAN.md for more scenarios`n" -ForegroundColor Yellow
