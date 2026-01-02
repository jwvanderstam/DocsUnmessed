@echo off
REM DocsUnmessed GUI - Build Executable (Windows Batch)
echo =====================================
echo DocsUnmessed GUI - Build Executable
echo =====================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell not found!
    echo Please install PowerShell or use Visual Studio to publish.
    pause
    exit /b 1
)

REM Run the PowerShell script
echo Running PowerShell build script...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0build-gui-executable.ps1"

echo.
echo Build process completed!
echo Check the output above for results.
echo.
pause
