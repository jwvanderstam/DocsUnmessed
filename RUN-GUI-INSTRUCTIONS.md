# Quick GUI Startup Guide

## ⚠️ Important: Save All Files First!

You have the following files open in Visual Studio:
- src\GUI\Resources\Styles\Styles.xaml
- src\GUI\Resources\Styles\Colors.xaml  
- src\GUI\Views\DashboardPage.xaml
- src\GUI\Views\AssessPage.xaml
- src\GUI\Views\MigrationPage.xaml
- src\GUI\Views\SettingsPage.xaml
- src\GUI\Platforms\Windows\App.xaml

## 📋 Steps to Run GUI:

### 1. **Save All Files in Visual Studio**
   - Press `Ctrl+Shift+S` (Save All)
   - Or: File → Save All

### 2. **Delete Obsolete Folder** (if it exists)
   - In Visual Studio Solution Explorer
   - Delete `src/GUI/Platforms` folder (old MAUI code)
   - Or delete it in File Explorer

### 3. **Build in Visual Studio**
   - Press `Ctrl+Shift+B`
   - Or: Build → Build Solution

### 4. **Run**
   - Press `F5` in Visual Studio
   - Or from command line:
     ```powershell
     dotnet run --project src\GUI\DocsUnmessed.GUI.csproj
     ```

## 🔍 If Build Fails:

### Check These:
1. All XAML files are saved
2. No syntax errors in XAML
3. `Platforms` folder is deleted
4. Clean solution: Build → Clean Solution, then Rebuild

### Expected Files:
```
src/GUI/Views/
├── DashboardPage.xaml
├── DashboardPage.xaml.cs
├── AssessPage.xaml
├── AssessPage.xaml.cs
├── MigrationPage.xaml
├── MigrationPage.xaml.cs
├── SettingsPage.xaml
└── SettingsPage.xaml.cs
```

## ✨ What You'll See:

When the GUI launches:
- Clean navigation sidebar on the left
- Dashboard page by default
- Navigate between pages using sidebar buttons
- Settings page has **CLI Documentation buttons**!

## 🎯 Features:

- 🏠 **Dashboard** - Welcome and quick start
- 📊 **Assess** - Scan files with real-time progress
- 🚀 **Migration** - Organize files (with dry-run preview)
- ⚙️ **Settings** - App settings + **CLI Docs Access**

---

**Save all files first, then build in Visual Studio!** 🎉
