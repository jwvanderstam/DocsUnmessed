# Build & Utility Scripts ???

Build scripts and utilities for the DocsUnmessed project.

## ?? Available Scripts

### Build Scripts

#### `build-gui-executable.ps1`
Build standalone Windows executable for the GUI.

```powershell
.\scripts\build-gui-executable.ps1
```

**Output**: `dist/DocsUnmessed-GUI-Windows/DocsUnmessed.GUI.exe`

---

#### `build-gui-executable.bat`
Batch wrapper for PowerShell build script. Double-click to run.

---

### Utility Scripts

#### `complete-gui-integration.ps1`
Helper script showing GUI integration status and remaining tasks.

```powershell
.\scripts\complete-gui-integration.ps1
```

---

## ?? Quick Reference

### Build Release
```powershell
.\scripts\build-gui-executable.ps1
```

### Output
```
dist/DocsUnmessed-GUI-Windows/
??? DocsUnmessed.GUI.exe
??? [runtime files]
```

## ?? Documentation

See [Build Guide](../docs/build/CREATE-EXECUTABLE-GUIDE.md) for detailed instructions.

---

**Status**: ? Ready
