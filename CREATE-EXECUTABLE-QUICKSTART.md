# ?? Create DocsUnmessed GUI Executable - Quick Start

## ? FASTEST METHOD (3 Steps)

### Option 1: PowerShell Script (Recommended)
```powershell
# From project root, run:
.\build-gui-executable.ps1
```

**Result**: Executable in `dist\DocsUnmessed-GUI-Windows\DocsUnmessed.GUI.exe`

### Option 2: Batch File (Double-Click)
```
1. Find: build-gui-executable.bat
2. Double-click it
3. Wait for completion
```

**Result**: Same as Option 1

### Option 3: Visual Studio 2022 (Most Reliable)
```
1. Right-click DocsUnmessed.GUI project
2. Select "Publish..."
3. Click "Publish" button
4. Find .exe in bin\Release\...\publish\
```

---

## ?? What You Get

After running any method:

```
dist\DocsUnmessed-GUI-Windows\
??? DocsUnmessed.GUI.exe  [80-150 MB]
    ? Self-contained (no .NET needed)
    ? Single file
    ? Ready to distribute
```

---

## ?? To Distribute

### Create ZIP Package
```powershell
# Add README to output folder
Copy-Item "dist\README.txt" "dist\DocsUnmessed-GUI-Windows\"

# Create ZIP
Compress-Archive `
    -Path "dist\DocsUnmessed-GUI-Windows\*" `
    -DestinationPath "DocsUnmessed-GUI-v1.0-Windows-x64.zip"
```

### Share With Users
```
1. Upload ZIP to GitHub Releases
2. Share download link
3. Users extract and run DocsUnmessed.GUI.exe
   (No installation required!)
```

---

## ? Files Created

| File | Purpose |
|------|---------|
| `build-gui-executable.ps1` | PowerShell build script |
| `build-gui-executable.bat` | Windows batch wrapper |
| `CREATE-EXECUTABLE-GUIDE.md` | Complete documentation |
| `dist/README.txt` | User instructions |
| `src/GUI/Properties/PublishProfiles/Windows-x64.pubxml` | VS publish profile |

---

## ?? Next Steps

### 1. Build the Executable
```powershell
.\build-gui-executable.ps1
```

### 2. Test It
```powershell
cd dist\DocsUnmessed-GUI-Windows
.\DocsUnmessed.GUI.exe
```

### 3. Create Distribution Package
```powershell
# Copy README
Copy-Item "dist\README.txt" "dist\DocsUnmessed-GUI-Windows\"

# Create ZIP
Compress-Archive -Path "dist\DocsUnmessed-GUI-Windows\*" -DestinationPath "DocsUnmessed-v1.0.zip"
```

### 4. Share!
Upload to GitHub Releases or share directly.

---

## ?? Quick Commands

### Build
```powershell
.\build-gui-executable.ps1
```

### Test
```powershell
dist\DocsUnmessed-GUI-Windows\DocsUnmessed.GUI.exe
```

### Package
```powershell
Compress-Archive -Path "dist\DocsUnmessed-GUI-Windows\*" -DestinationPath "DocsUnmessed-v1.0.zip"
```

### Clean
```powershell
Remove-Item dist -Recurse -Force
Remove-Item src\GUI\bin -Recurse -Force
Remove-Item src\GUI\obj -Recurse -Force
```

---

## ?? Summary

**Created**:
- ? Build scripts (PowerShell + Batch)
- ? Publish profile for Visual Studio
- ? Complete documentation
- ? User README
- ? Distribution guide

**To Create Executable**:
```powershell
.\build-gui-executable.ps1
```

**Result**:
- ? Single .exe file (80-150 MB)
- ? Self-contained (no .NET required)
- ? Ready to distribute
- ? Works on Windows 10/11

**Ready to build and share!** ??

