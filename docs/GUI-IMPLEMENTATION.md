# DocsUnmessed GUI - Implementation Guide

## Overview

A modern, cross-platform desktop GUI for DocsUnmessed built with **.NET MAUI** (Multi-platform App UI).

---

## ?? **What Has Been Created**

### Project Structure ?

```
src/GUI/
??? DocsUnmessed.GUI.csproj     # MAUI project file
??? App.xaml                     # Application entry point
??? App.xaml.cs
??? MauiProgram.cs              # Dependency injection & services
??? AppShell.xaml               # Navigation shell
??? AppShell.xaml.cs
?
??? ViewModels/                  # MVVM ViewModels
?   ??? DashboardViewModel.cs   # ? Complete
?   ??? AssessViewModel.cs      # ?? To implement
?   ??? MigrationViewModel.cs   # ?? To implement
?   ??? SettingsViewModel.cs    # ?? To implement
?
??? Views/                       # XAML Pages
?   ??? DashboardPage.xaml      # ? Complete
?   ??? DashboardPage.xaml.cs
?   ??? AssessPage.xaml         # ?? To implement
?   ??? MigrationPage.xaml      # ?? To implement
?   ??? SettingsPage.xaml       # ?? To implement
?
??? Resources/                   # Assets & styles
    ??? Styles/
    ?   ??? Colors.xaml
    ?   ??? Styles.xaml
    ??? Fonts/
    ??? Images/
    ??? AppIcon/
```

---

## ?? **NuGet Packages Required**

```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="10.0.0" />
<PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="10.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="CommunityToolkit.Maui" Version="10.0.0" />
```

---

## ?? **UI Features**

### Dashboard Page ? **COMPLETE**

**Features**:
- Welcome message
- Statistics cards (Total Scans, Files, Size)
- Quick action buttons
- Recent scans list
- Responsive layout

**Navigation**:
- Quick access to Assess and Migration pages
- Shows recent scan history

### Assess Page ?? **TO IMPLEMENT**

**Features to Add**:
- Directory picker
- Provider selection
- Exclusion options
- Scan button
- Real-time progress
- Results display

### Migration Page ?? **TO IMPLEMENT**

**Features to Add**:
- Scan selector
- Category preview
- Dry-run toggle
- Structure preservation option
- Execute button
- Progress bar with stats

### Settings Page ?? **TO IMPLEMENT**

**Features to Add**:
- Default exclusions configuration
- Target directory settings
- Conflict resolution preferences
- Performance options

---

## ?? **To Complete the GUI**

### Step 1: Create Remaining Stub Files

Create placeholder files for:
1. `AssessViewModel.cs`
2. `AssessPage.xaml` + `.xaml.cs`
3. `MigrationViewModel.cs`
4. `MigrationPage.xaml` + `.xaml.cs`
5. `SettingsViewModel.cs`
6. `SettingsPage.xaml` + `.xaml.cs`

### Step 2: Create Style Resources

**Colors.xaml**:
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
    <Color x:Key="Primary">#512BD4</Color>
    <Color x:Key="Secondary">#DFD8F7</Color>
    <Color x:Key="Success">#28a745</Color>
    <Color x:Key="Danger">#dc3545</Color>
    <Color x:Key="Warning">#ffc107</Color>
    <Color x:Key="Info">#17a2b8</Color>
</ResourceDictionary>
```

**Styles.xaml**:
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
    <Style TargetType="Button">
        <Setter Property="CornerRadius" Value="8"/>
        <Setter Property="HeightRequest" Value="44"/>
    </Style>
    
    <Style TargetType="Frame">
        <Setter Property="HasShadow" Value="True"/>
        <Setter Property="CornerRadius" Value="10"/>
    </Style>
</ResourceDictionary>
```

### Step 3: Build and Run

```sh
# Restore packages
dotnet restore src/GUI/DocsUnmessed.GUI.csproj

# Build GUI project
dotnet build src/GUI/DocsUnmessed.GUI.csproj

# Run on Windows
dotnet run --project src/GUI/DocsUnmessed.GUI.csproj --framework net10.0-windows10.0.19041.0
```

---

## ?? **Current Implementation Status**

### ? **Complete** (40%):
- [x] Project structure
- [x] MAUI configuration
- [x] Dependency injection
- [x] AppShell navigation
- [x] Dashboard ViewModel
- [x] Dashboard View (complete UI)
- [x] Service registration
- [x] Database integration

### ?? **To Implement** (60%):
- [ ] AssessViewModel + View
- [ ] MigrationViewModel + View
- [ ] SettingsViewModel + View
- [ ] Style resources
- [ ] Icon assets
- [ ] Font resources
- [ ] Testing on platforms

---

## ?? **Implementation Guide**

### For AssessPage:

**ViewModel**:
```csharp
public partial class AssessViewModel : ObservableObject
{
    [ObservableProperty]
    private string rootPath = "";
    
    [ObservableProperty]
    private bool isScanning;
    
    [ObservableProperty]
    private int filesScanned;
    
    [RelayCommand]
    private async Task BrowseDirectoryAsync()
    {
        // File picker logic
    }
    
    [RelayCommand]
    private async Task StartScanAsync()
    {
        IsScanning = true;
        // Call inventory service
        IsScanning = false;
    }
}
```

**View**:
```xml
<Entry Text="{Binding RootPath}" Placeholder="Select directory..."/>
<Button Text="Browse" Command="{Binding BrowseDirectoryCommand}"/>
<Button Text="Start Scan" Command="{Binding StartScanCommand}"/>
<ProgressBar Progress="{Binding ScanProgress}"/>
<Label Text="{Binding FilesScanned}"/>
```

### For MigrationPage:

**ViewModel**:
```csharp
public partial class MigrationViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedScanId = "";
    
    [ObservableProperty]
    private bool isDryRun = true;
    
    [ObservableProperty]
    private bool preserveStructure = true;
    
    [RelayCommand]
    private async Task ExecuteMigrationAsync()
    {
        // Call migration command
    }
}
```

---

## ?? **Running the GUI**

### Windows:
```sh
dotnet run --project src/GUI/DocsUnmessed.GUI.csproj -f net10.0-windows10.0.19041.0
```

### macOS:
```sh
dotnet run --project src/GUI/DocsUnmessed.GUI.csproj -f net10.0-maccatalyst
```

---

## ?? **Architecture**

### MVVM Pattern

```
View (XAML)
    ? (Data Binding)
ViewModel (ObservableObject)
    ? (Commands)
Services (IInventoryService, etc.)
    ?
Database/Core Logic
```

### Benefits:
- ? Separation of concerns
- ? Testable ViewModels
- ? Reactive UI updates
- ? Clean code structure

---

## ?? **UI Preview**

### Dashboard:
```
???????????????????????????????????????????
?  DocsUnmessed                     [?]   ?
???????????????????????????????????????????
?                                         ?
?  Welcome to DocsUnmessed!              ?
?  Your file organization assistant       ?
?                                         ?
???????????????????????????????????????????
?   ??          ??          ??            ?
?    5          1,234       5.2 GB        ?
?  Scans       Files       Size           ?
???????????????????????????????????????????
?                                         ?
?  Quick Actions                          ?
?  [?? Assess Files] [?? Migrate Files]  ?
?                                         ?
?  Recent Scans                           ?
?  ????????????????????????????????????? ?
?  ? ?? abc123     2025-01-15  1,234   ? ?
?  ????????????????????????????????????? ?
???????????????????????????????????????????
```

---

## ?? **Next Steps**

### Priority 1: Complete Stub Files
```sh
# Create remaining ViewModels
touch src/GUI/ViewModels/AssessViewModel.cs
touch src/GUI/ViewModels/MigrationViewModel.cs
touch src/GUI/ViewModels/SettingsViewModel.cs

# Create remaining Views
touch src/GUI/Views/AssessPage.xaml
touch src/GUI/Views/MigrationPage.xaml
touch src/GUI/Views/SettingsPage.xaml
```

### Priority 2: Style Resources
```sh
mkdir -p src/GUI/Resources/Styles
touch src/GUI/Resources/Styles/Colors.xaml
touch src/GUI/Resources/Styles/Styles.xaml
```

### Priority 3: Test & Run
```sh
dotnet build src/GUI/DocsUnmessed.GUI.csproj
dotnet run --project src/GUI/DocsUnmessed.GUI.csproj
```

---

## ?? **Resources**

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [CommunityToolkit.Maui](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)

---

## ? **Benefits of This GUI**

1. **Cross-Platform**: Windows, macOS, iOS, Android
2. **Modern**: .NET MAUI with Material Design
3. **Reactive**: MVVM with CommunityToolkit
4. **Integrated**: Uses existing DocsUnmessed services
5. **Extensible**: Easy to add new views
6. **Professional**: Clean architecture

---

*GUI Implementation Guide - Day 35*  
*Status: Foundation Complete (40%)*  
*Next: Implement remaining pages*  
*Target: Full GUI by Day 37*

