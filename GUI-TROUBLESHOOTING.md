# ?? GUI Troubleshooting Guide

## The GUI didn't appear - let's fix it!

### Method 1: Run from Visual Studio (Recommended)

1. **Close the open `Platforms\Windows\App.xaml` file** (it's obsolete MAUI code)
2. **In Solution Explorer**, right-click `DocsUnmessed.GUI` project
3. Click **"Set as Startup Project"**
4. Press **F5** (or click the green ?? button)

### Method 2: Check for Error Dialog

The GUI might have shown an error dialog. Common issues:

**If you see an error about ViewModels:**
- The DashboardViewModel might need a parameterless constructor
- Check the error message and let me know

**If you see an error about "Views namespace":**
- Make sure all 4 XAML files are saved
- Try: Build ? Clean Solution, then Build ? Rebuild

### Method 3: Manual Executable

Run this in PowerShell:
```powershell
cd C:\Users\Gebruiker\OneDrive\GitHub\DocsUnmessed
.\src\GUI\bin\Debug\net10.0-windows\DocsUnmessed.GUI.exe
```

If an error appears, copy the message!

### Method 4: Check Saved Files

Verify these files exist and have content:
```
src\GUI\Views\DashboardPage.xaml
src\GUI\Views\AssessPage.xaml  
src\GUI\Views\MigrationPage.xaml
src\GUI\Views\SettingsPage.xaml
```

Each should be ~2-3 KB in size.

### Common Issues:

#### Issue 1: DashboardViewModel Constructor
The ViewModel might need a fix. Check if you see this error:
```
"No parameterless constructor"
```

**Fix:** Let me know and I'll update the ViewModel.

#### Issue 2: Files Not Saved
- Press `Ctrl+Shift+S` in VS to save all
- Close and reopen Visual Studio

#### Issue 3: Obsolete Platforms Folder
- Delete the entire `src\GUI\Platforms` folder
- It's old MAUI code conflicting with WPF

### Quick Diagnostic:

Run this command and tell me what you see:
```powershell
Get-ChildItem "src\GUI\Views\*.xaml" | ForEach-Object { "$($_.Name) - $($_.Length) bytes" }
```

### What Should Work:

Once running, you should see:
- Window titled "DocsUnmessed - Document Organization Tool"
- Dark sidebar on the left with 4 buttons
- Dashboard content on the right
- Clean, modern interface

---

**Next Steps:**
1. Try Method 1 (VS F5) first
2. If error appears, tell me the exact message
3. If nothing happens, run the diagnostic command above

Let me know what happens! ??
