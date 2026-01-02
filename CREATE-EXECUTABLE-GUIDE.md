# DocsUnmessed GUI - Create Executable Guide

## ?? Goal
Create a standalone Windows executable (.exe) that can run without installing .NET.

---

## ?? Option 1: Using PowerShell Script (Recommended)

### Quick Method
```powershell
# Run this in PowerShell from the project root:
.\build-gui-executable.ps1
```

### What It Does
1. ? Cleans previous builds
2. ? Restores NuGet packages
3. ? Publishes as self-contained executable
4. ? Creates single-file .exe
5. ? Outputs to `dist\DocsUnmessed-GUI-Windows\`

### Result
```
dist\DocsUnmessed-GUI-Windows\
??? DocsUnmessed.GUI.exe  <-- Main executable (80-150 MB)
??? docsunmessed.db       <-- Database (if exists)
??? [Other runtime files]
```

### To Distribute
```powershell
# Zip the entire folder
Compress-Archive -Path "dist\DocsUnmessed-GUI-Windows" -DestinationPath "DocsUnmessed-GUI-v1.0.zip"
```

---

## ?? Option 2: Using Visual Studio 2022 (Easiest)

### Step-by-Step

#### 1. Open Publish Dialog
```
Right-click "DocsUnmessed.GUI" project in Solution Explorer
? Select "Publish..."
```

#### 2. Create New Profile (First Time Only)
```
Click "New" or "Add a publish profile"
? Target: Folder
? Location: bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\
? Click "Finish"
```

#### 3. Configure Publish Settings
```
Click "Show all settings" or "Edit"

Configuration: Release
Target framework: net10.0-windows10.0.19041.0
Deployment mode: Self-contained
Target runtime: win-x64

Advanced settings:
? Produce single file
? Enable ReadyToRun compilation
? Trim unused code (optional - may reduce size)
```

#### 4. Publish
```
Click "Publish" button
Wait 30-60 seconds
```

#### 5. Find Your Executable
```
Location: src\GUI\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\
File: DocsUnmessed.GUI.exe
```

---

## ?? Option 3: Using Command Line

### Self-Contained Executable (Recommended)
```powershell
cd "C:\Users\Gebruiker\OneDrive\GitHub\DocsUnmessed"

dotnet publish src/GUI/DocsUnmessed.GUI.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output dist/DocsUnmessed-GUI-Windows `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishReadyToRun=true
```

### Framework-Dependent (Smaller, requires .NET 10)
```powershell
dotnet publish src/GUI/DocsUnmessed.GUI.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output dist/DocsUnmessed-GUI-Windows-FD `
    /p:PublishSingleFile=true
```

---

## ?? Executable Types Comparison

| Type | Size | Requires .NET | Best For |
|------|------|---------------|----------|
| **Self-Contained** | 80-150 MB | ? No | Distribution to others |
| **Framework-Dependent** | 10-20 MB | ? Yes | Personal use |
| **Trimmed** | 40-80 MB | ? No | Advanced users |

---

## ? Verification Steps

### 1. Check File Exists
```powershell
Test-Path "dist\DocsUnmessed-GUI-Windows\DocsUnmessed.GUI.exe"
# Should return: True
```

### 2. Check File Size
```powershell
$file = Get-Item "dist\DocsUnmessed-GUI-Windows\DocsUnmessed.GUI.exe"
[math]::Round($file.Length / 1MB, 2)
# Should show: 80-150 MB (self-contained)
```

### 3. Test Run
```powershell
cd dist\DocsUnmessed-GUI-Windows
.\DocsUnmessed.GUI.exe
```

### Expected Result
- ? Window opens
- ? Dashboard loads
- ? No errors
- ? Can navigate pages

---

## ?? Distribution

### For End Users (No Installation Required)

#### Create ZIP Package
```powershell
# From project root
Compress-Archive `
    -Path "dist\DocsUnmessed-GUI-Windows\*" `
    -DestinationPath "DocsUnmessed-GUI-v1.0-Windows-x64.zip"
```

#### Share Instructions
```
1. Extract ZIP to any folder
2. Double-click DocsUnmessed.GUI.exe
3. That's it! No installation needed.

Requirements:
- Windows 10 version 19041 or higher
- Windows 11 (any version)
```

### For Advanced Users

#### Create Installer (Optional)
Use tools like:
- **Inno Setup** (free, recommended)
- **WiX Toolset** (free, advanced)
- **Advanced Installer** (paid, easy)

---

## ?? Troubleshooting

### Build Fails with Resizetizer Error
**Solution:** Use Visual Studio 2022 instead of command line
```
Visual Studio handles MAUI build better than dotnet CLI
```

### Executable Too Large
**Solution:** Enable trimming (advanced)
```powershell
dotnet publish ... /p:PublishTrimmed=true
```
?? Warning: May break some features, test thoroughly!

### Missing Dependencies
**Solution:** Use self-contained mode
```powershell
--self-contained true
```

### Database Not Included
**Solution:** Copy manually after build
```powershell
Copy-Item "docsunmessed.db" "dist\DocsUnmessed-GUI-Windows\"
```

---

## ?? Build Output Structure

### Self-Contained Build
```
dist\DocsUnmessed-GUI-Windows\
??? DocsUnmessed.GUI.exe         [80-150 MB] Main executable
??? DocsUnmessed.GUI.pdb         [Debug symbols]
??? [.NET runtime files bundled inside .exe]
```

### Framework-Dependent Build
```
dist\DocsUnmessed-GUI-Windows-FD\
??? DocsUnmessed.GUI.exe         [10-20 MB] Main executable
??? DocsUnmessed.GUI.dll         [Application code]
??? DocsUnmessed.GUI.pdb         [Debug symbols]
??? [Various .dll dependencies]
```

---

## ?? Recommended Build Settings

### For Distribution (Most Users)
```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishReadyToRun>true</PublishReadyToRun>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

### For Personal Use (Smaller)
```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>false</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
</PropertyGroup>
```

---

## ?? Quick Reference

### PowerShell Script (Easiest)
```powershell
.\build-gui-executable.ps1
```

### Visual Studio (Most Reliable)
```
Right-click project ? Publish ? Publish
```

### Command Line (Manual)
```powershell
dotnet publish src/GUI/DocsUnmessed.GUI.csproj -c Release -r win-x64 --self-contained true -o dist/DocsUnmessed /p:PublishSingleFile=true
```

---

## ? Checklist

Before distributing:
- [ ] Executable builds successfully
- [ ] File size is reasonable (80-150 MB)
- [ ] Executable runs on your machine
- [ ] Executable runs on clean test machine (no .NET)
- [ ] All 4 pages load correctly
- [ ] Navigation works
- [ ] Database functions work
- [ ] README.txt included
- [ ] License file included (if applicable)
- [ ] Version number correct

---

## ?? Distribution Package

### Recommended Contents
```
DocsUnmessed-GUI-v1.0-Windows-x64.zip
??? DocsUnmessed.GUI.exe
??? README.txt
??? LICENSE.txt
??? QUICK-START.txt
```

### README.txt Template
```txt
DocsUnmessed GUI v1.0
=====================

Privacy-first file organization tool for Windows.

HOW TO RUN:
1. Extract this ZIP to any folder
2. Double-click DocsUnmessed.GUI.exe
3. That's it!

REQUIREMENTS:
- Windows 10 (version 19041 or higher)
- Windows 11 (any version)
- No .NET installation required!

FEATURES:
- Scan local directories
- Organize files by category
- Multi-cloud support (OneDrive, Google Drive, Dropbox, iCloud)
- Smart migration with preview
- Non-destructive operations

For help, visit: [Your Website/GitHub]
```

---

## ?? Success!

After running any of the build methods, you'll have:

? **DocsUnmessed.GUI.exe** - Standalone executable
? **No installation required** - Just run the .exe
? **Self-contained** - Includes .NET runtime
? **Single file** - Easy to distribute

**Ready to share with the world!** ??

---

*Executable Creation Guide*  
*DocsUnmessed GUI v1.0*  
*Status: Production Ready*

