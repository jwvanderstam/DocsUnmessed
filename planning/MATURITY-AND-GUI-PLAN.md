# DocsUnmessed Maturity & GUI Development Plan

## Executive Summary

This document outlines a comprehensive plan to mature the DocsUnmessed application from its current Phase 1 foundation to a production-ready application with a professional cross-platform GUI.

**Current State**: Phase 1 complete with working CLI, file system scanning, and architectural foundation.

**Target State**: Enterprise-grade file organization tool with intuitive GUI, cloud provider support, and production features.

**Timeline**: 12-16 weeks for full maturity + GUI implementation.

---

## Phase 2: Core Feature Completion (Weeks 1-4)

### 2.1 Rules Engine Implementation

**Objective**: Complete the pattern-matching and target suggestion system.

**Tasks**:

1. **Rule Matching Engine**
   - Implement regex-based path matching
   - File extension matching
   - MIME type filtering
   - Date range filtering (age-based rules)
   - File size range matching
   - Keyword-based matching
   - Priority-based rule evaluation

2. **Target Suggestion Generator**
   - Token extraction from file paths/names
   - Dynamic path generation with placeholders
   - EXIF date extraction for photos (optional)
   - Office document metadata (optional)
   - Confidence scoring
   - Explainability (why this suggestion?)

3. **Conflict Resolution**
   - Version suffix strategy (`file_v01.txt`)
   - Timestamp suffix strategy (`file_20250103123045.txt`)
   - Manual queue for ambiguous cases
   - Collision detection and prevention

**Deliverables**:
- `src/Services/RulesEngine.cs`
- `src/Services/Rules/` (pattern matchers)
- `src/Services/TargetSuggestionGenerator.cs`
- Unit tests for rule evaluation
- Example rule configurations

**Acceptance Criteria**:
- ? 95%+ rule match accuracy
- ? <100ms evaluation time per file
- ? Handles complex regex patterns
- ? Explainable suggestions

---

### 2.2 SQLite Persistence Layer

**Objective**: Replace in-memory storage with durable SQLite database.

**Tasks**:

1. **Database Schema Design**
   ```sql
   -- Scans table
   CREATE TABLE scans (
       id TEXT PRIMARY KEY,
       started_utc TEXT NOT NULL,
       completed_utc TEXT,
       providers TEXT NOT NULL
   );
   
   -- Items table
   CREATE TABLE items (
       id TEXT PRIMARY KEY,
       scan_id TEXT NOT NULL,
       path TEXT NOT NULL,
       provider TEXT NOT NULL,
       size INTEGER NOT NULL,
       hash TEXT,
       created_utc TEXT,
       modified_utc TEXT,
       -- ... more fields
       FOREIGN KEY (scan_id) REFERENCES scans(id)
   );
   
   -- Add indexes for performance
   CREATE INDEX idx_items_scan ON items(scan_id);
   CREATE INDEX idx_items_hash ON items(hash);
   ```

2. **Repository Implementation**
   - `SqliteInventoryService` implementing `IInventoryService`
   - Connection pooling
   - Transaction support
   - Batch inserts for performance
   - Query optimization

3. **Migration from In-Memory**
   - Database initialization
   - Schema versioning
   - Migration scripts
   - Configuration for DB location

**Deliverables**:
- `src/Services/SqliteInventoryService.cs`
- `src/Infrastructure/Database/` (schema, migrations)
- NuGet: `Microsoft.Data.Sqlite`
- Integration tests with real database
- Performance benchmarks

**Acceptance Criteria**:
- ? Handles 500k+ items efficiently
- ? <1s query response time
- ? Transaction safety
- ? Database migration support

---

### 2.3 Naming Template Engine

**Objective**: Parse and apply naming templates to files.

**Tasks**:

1. **Template Parser**
   - Parse template strings: `{YYYY}{MM}{DD}_{Context}_{Title}_v{NN}.{ext}`
   - Token extraction and validation
   - Required vs optional tokens
   - Default value handling

2. **Token Expanders**
   - Date/time tokens (YYYY, MM, DD, HH, mm, ss)
   - Context tokens (user-defined)
   - Auto-increment version numbers (NN)
   - File extension preservation
   - EXIF date extraction (photos)

3. **Normalization Rules**
   - Case transformation (TitleCase, lowercase, UPPERCASE)
   - Separator handling (spaces to underscores)
   - Stopword removal (the, a, an, etc.)
   - Invalid character replacement
   - Length truncation

**Deliverables**:
- `src/Services/NamingTemplateEngine.cs`
- `src/Services/TokenExpanders/` (date, context, version)
- Template validation
- Unit tests for all token types

**Acceptance Criteria**:
- ? Supports all PRD template patterns
- ? Handles edge cases (missing tokens)
- ? Validates templates before use
- ? Extensible for custom tokens

---

### 2.4 Duplicate Detection Enhancement

**Objective**: Implement advanced duplicate detection beyond exact hash matching.

**Tasks**:

1. **Probabilistic Matching**
   - Levenshtein distance for similar names
   - Configurable similarity threshold (default 85%)
   - Name + size matching
   - Fuzzy date matching (±1 day)

2. **Partial Hash Comparison**
   - Block hashing for large files
   - First/last N bytes comparison
   - Configurable block size

3. **Duplicate Groups**
   - Group by exact hash
   - Group by probable match
   - Recommend which to keep (newest, largest, etc.)
   - Preview before deletion

**Deliverables**:
- `src/Services/DuplicateDetectionService.cs`
- Levenshtein algorithm implementation
- Configuration for thresholds
- Unit tests with sample duplicates

**Acceptance Criteria**:
- ? Detects exact duplicates (100% accuracy)
- ? Detects probable duplicates (configurable threshold)
- ? No false positives on unique files
- ? Performance: <5 seconds for 10k files

---

## Phase 3: Migration Execution (Weeks 5-6)

### 3.1 Migration Orchestrator

**Objective**: Complete the migration execution engine with production features.

**Tasks**:

1. **Batch Processing**
   - Configurable batch size (default 500)
   - Parallel execution with throttling
   - Rate limiting per provider
   - Memory-efficient streaming

2. **Checkpointing & Resume**
   - Save state after each batch
   - Resume from last checkpoint
   - Handle interruptions gracefully
   - Rollback support

3. **Verification**
   - Hash verification after copy/move
   - Size verification
   - Path validation
   - Quarantine for failed operations

4. **Audit Logging**
   - JSONL format audit trail
   - Per-operation logging
   - Correlation IDs for tracking
   - Timestamps and status

**Deliverables**:
- Complete `src/Services/MigrationOrchestrator.cs`
- `src/Services/BatchProcessor.cs`
- `src/Services/AuditLogger.cs`
- Checkpoint persistence
- Integration tests

**Acceptance Criteria**:
- ? Handles 100k+ file migrations
- ? Resume from interruption
- ? 100% hash verification
- ? Complete audit trail

---

### 3.2 Validate Command

**Objective**: Implement the validate command for structure and naming validation.

**Tasks**:

1. **Structure Validation**
   - Max depth checking
   - File count per folder
   - Numeric prefix validation
   - Reserved name detection

2. **Naming Validation**
   - Check against templates
   - Invalid character detection
   - Path length validation
   - Report violations

3. **Auto-Fix**
   - Batch rename operations
   - Flatten deep hierarchies (with user approval)
   - Fix invalid characters
   - Dry-run mode

**Deliverables**:
- `src/CLI/Commands/ValidateCommand.cs`
- Validation report format
- Auto-fix engine
- CLI integration

**Acceptance Criteria**:
- ? Validates against TIA blueprint
- ? Validates against naming templates
- ? Reports all violations
- ? Auto-fix works safely

---

## Phase 4: Cloud Provider Connectors (Weeks 7-9)

### 4.1 OneDrive Connector

**Objective**: Support OneDrive Personal and OneDrive for Business.

**Tasks**:

1. **Microsoft Graph Integration**
   - OAuth 2.0 authentication flow
   - Token storage in OS keychain
   - Token refresh handling
   - Rate limiting (per Microsoft Graph limits)

2. **Enumeration**
   - Recursive folder traversal via Graph API
   - Delta queries for incremental scans
   - Metadata extraction
   - Large file handling (streaming)

3. **Operations**
   - Upload/download via Graph API
   - Move/copy/rename operations
   - Shortcut creation
   - Conflict resolution

4. **Local Sync Folder Support**
   - Detect OneDrive sync folder
   - Enumerate local files
   - Detect sync status
   - Avoid sync conflicts

**Deliverables**:
- `src/Connectors/OneDrive/OneDriveApiConnector.cs`
- `src/Connectors/OneDrive/OneDriveLocalConnector.cs`
- `src/Connectors/OneDrive/GraphAuthProvider.cs`
- NuGet: `Microsoft.Graph`, `Microsoft.Identity.Client`
- OAuth flow implementation
- Integration tests with test tenant

**Acceptance Criteria**:
- ? OAuth authentication works
- ? Enumerate 50k+ files
- ? Respects Graph API rate limits
- ? Handles large files (>4GB)

---

### 4.2 Google Drive Connector

**Objective**: Support Google Drive via Drive API and local sync.

**Tasks**:

1. **Google Drive API Integration**
   - OAuth 2.0 authentication
   - Drive API v3
   - File listing and metadata
   - Upload/download operations

2. **Local Sync Support**
   - Detect Google Drive sync folder
   - Enumerate local files
   - Map local to cloud paths

**Deliverables**:
- `src/Connectors/GoogleDrive/GoogleDriveApiConnector.cs`
- `src/Connectors/GoogleDrive/GoogleDriveLocalConnector.cs`
- NuGet: `Google.Apis.Drive.v3`
- OAuth flow

**Acceptance Criteria**:
- ? OAuth works with Google
- ? Enumerate Drive files
- ? Upload/download operations
- ? Rate limiting

---

### 4.3 Dropbox Connector

**Objective**: Support Dropbox via API v2 and local sync.

**Tasks**:

1. **Dropbox API v2 Integration**
   - OAuth 2.0 authentication
   - File listing via API
   - Upload/download operations
   - Rate limiting

2. **Local Sync Support**
   - Detect Dropbox folder
   - Enumerate local files

**Deliverables**:
- `src/Connectors/Dropbox/DropboxApiConnector.cs`
- `src/Connectors/Dropbox/DropboxLocalConnector.cs`
- NuGet: `Dropbox.Api`

**Acceptance Criteria**:
- ? OAuth works
- ? Enumerate files
- ? Upload/download

---

### 4.4 iCloud Drive Connector

**Objective**: Support iCloud Drive via local sync folder (no public API).

**Tasks**:

1. **Local Enumeration**
   - Detect iCloud Drive folder (macOS, Windows)
   - Enumerate local files
   - Handle sync status metadata

**Deliverables**:
- `src/Connectors/ICloud/ICloudLocalConnector.cs`

**Acceptance Criteria**:
- ? Detects iCloud folder
- ? Enumerates files
- ? Respects sync status

---

## Phase 5: Testing & Quality (Weeks 10-11)

### 5.1 Unit Testing

**Objective**: Achieve 80%+ code coverage with unit tests.

**Tasks**:

1. **Domain Model Tests**
   - Item validation
   - Operation state transitions
   - ScanResult aggregation

2. **Service Tests**
   - Rules engine evaluation
   - Hash computation
   - Path validation
   - Duplicate detection

3. **Mock Providers**
   - Mock connector implementations
   - Test data generators

**Deliverables**:
- `tests/DocsUnmessed.Tests.Unit/` (complete suite)
- NuGet: `xUnit`, `Moq`, `FluentAssertions`
- CI integration (GitHub Actions)
- Coverage reports

**Acceptance Criteria**:
- ? 80%+ code coverage
- ? All critical paths tested
- ? Fast test execution (<30s)

---

### 5.2 Integration Testing

**Objective**: Test end-to-end scenarios with real components.

**Tasks**:

1. **File System Tests**
   - Real directory scanning
   - File operations (copy, move, rename)
   - Hash verification

2. **Database Tests**
   - SQLite persistence
   - Transaction handling
   - Query performance

3. **CLI Tests**
   - Command execution
   - Argument parsing
   - Output validation

**Deliverables**:
- `tests/DocsUnmessed.Tests.Integration/`
- Test fixtures
- Performance benchmarks

**Acceptance Criteria**:
- ? All integration tests pass
- ? Performance benchmarks met
- ? No memory leaks

---

## Phase 6: GUI Architecture (Week 12)

### 6.1 Shared API Layer

**Objective**: Extract core logic into shared library for CLI and GUI.

**Tasks**:

1. **Refactor Project Structure**
   ```
   DocsUnmessed/
   ??? src/
   ?   ??? DocsUnmessed.Core/          # Domain, interfaces
   ?   ??? DocsUnmessed.Infrastructure/ # Connectors, persistence
   ?   ??? DocsUnmessed.Application/    # Services, orchestration
   ?   ??? DocsUnmessed.CLI/            # CLI entry point
   ?   ??? DocsUnmessed.GUI/            # .NET MAUI app
   ```

2. **Application Services**
   - `IAssessmentService` - Scan orchestration
   - `IMigrationService` - Migration orchestration
   - `IValidationService` - Structure validation
   - `IConfigurationService` - Config management

3. **Shared Models**
   - DTOs for cross-layer communication
   - View models for presentation
   - Command/query objects

**Deliverables**:
- Multi-project solution structure
- Shared libraries
- Service abstractions
- Updated documentation

**Acceptance Criteria**:
- ? Clean separation of concerns
- ? CLI and GUI share same core
- ? No code duplication
- ? Maintainable architecture

---

### 6.2 GUI Technology Selection

**Recommendation**: **.NET MAUI** (Multi-platform App UI)

**Rationale**:
- ? Native .NET 10 support
- ? Cross-platform (Windows, macOS, iOS, Android)
- ? Modern UI framework (XAML)
- ? MVVM pattern support
- ? Leverages existing C# codebase
- ? Hot reload for fast development
- ? Native performance

**Alternative Considered**: Avalonia, Electron
- Avalonia: Good but smaller ecosystem
- Electron: Heavy runtime, not native .NET

**Decision**: .NET MAUI for native experience and C# integration.

---

## Phase 7: GUI Core Implementation (Weeks 13-14)

### 7.1 Application Shell

**Objective**: Create main application structure with navigation.

**Tasks**:

1. **Main Window**
   - Shell with hamburger menu
   - Navigation bar
   - Status bar
   - Settings flyout

2. **Navigation**
   - Dashboard
   - Assessment
   - Migration
   - Validation
   - Settings

3. **Theming**
   - Light/dark mode
   - Custom color schemes
   - Accessibility support

**Deliverables**:
- `src/DocsUnmessed.GUI/Shell.xaml`
- Navigation infrastructure
- Theme resources

**Acceptance Criteria**:
- ? Smooth navigation
- ? Responsive layout
- ? Keyboard shortcuts

---

### 7.2 Dashboard View

**Objective**: Overview of storage status and quick actions.

**Tasks**:

1. **KPI Cards**
   - Total files
   - Total size
   - Duplicates found
   - Issues detected
   - Provider breakdown

2. **Charts**
   - Size by provider (pie chart)
   - Files by type (bar chart)
   - Depth distribution (histogram)

3. **Quick Actions**
   - Start assessment
   - View last scan
   - Quick migration
   - Settings shortcut

**Deliverables**:
- `Views/DashboardPage.xaml`
- `ViewModels/DashboardViewModel.cs`
- NuGet: `LiveChartsCore` (charts)

**Acceptance Criteria**:
- ? Real-time updates
- ? Interactive charts
- ? Clear KPIs

---

### 7.3 Assessment View

**Objective**: Configure and run storage assessments with progress tracking.

**Tasks**:

1. **Configuration Panel**
   - Provider selection (checkboxes)
   - Root path selection (folder picker)
   - Filter options (extensions, size, dates)
   - Hash computation toggle

2. **Progress View**
   - Real-time progress bar
   - Items processed counter
   - Current file display
   - Pause/cancel buttons

3. **Results View**
   - Summary statistics
   - Provider breakdown
   - Issue list
   - Export options (JSON, CSV, PDF)

**Deliverables**:
- `Views/AssessmentPage.xaml`
- `ViewModels/AssessmentViewModel.cs`
- Progress reporting infrastructure

**Acceptance Criteria**:
- ? Clear configuration
- ? Real-time progress
- ? Detailed results

---

### 7.4 File Browser View

**Objective**: Visual tree comparison of current vs target structure.

**Tasks**:

1. **Split View**
   - Left: Current structure
   - Right: Target structure
   - Sync scrolling

2. **Tree Controls**
   - Expandable folders
   - File icons by type
   - Size and date display
   - Issue indicators

3. **Diff Highlighting**
   - Files to move (yellow)
   - Files to rename (blue)
   - Duplicates (red)
   - Compliant (green)

4. **Actions**
   - Right-click context menu
   - Drag-and-drop preview
   - Exclude from migration
   - Override suggestions

**Deliverables**:
- `Views/FileBrowserPage.xaml`
- `ViewModels/FileBrowserViewModel.cs`
- Tree virtualization for performance

**Acceptance Criteria**:
- ? Handles 100k+ files
- ? Fast rendering
- ? Clear diff indicators
- ? Interactive actions

---

## Phase 8: GUI Advanced Features (Weeks 15-16)

### 8.1 Migration Planning View

**Objective**: Review and customize migration plans before execution.

**Tasks**:

1. **Plan Summary**
   - Total operations
   - Operations by type
   - Estimated time
   - Metrics (depth reduction, duplicates eliminated)

2. **Operation List**
   - Sortable/filterable table
   - Source ? Target display
   - Operation type icons
   - Conflict indicators

3. **Customization**
   - Override individual targets
   - Exclude specific files
   - Change conflict resolution
   - Save/load plans

**Deliverables**:
- `Views/MigrationPlanningPage.xaml`
- `ViewModels/MigrationPlanningViewModel.cs`
- Plan serialization

**Acceptance Criteria**:
- ? Clear plan overview
- ? Easy customization
- ? Plan persistence

---

### 8.2 Migration Execution View

**Objective**: Monitor and control migration execution.

**Tasks**:

1. **Real-Time Progress**
   - Overall progress bar
   - Current batch progress
   - Operations per second
   - Time remaining estimate

2. **Status Display**
   - Completed operations (count)
   - Failed operations (list)
   - Skipped operations
   - Current operation detail

3. **Controls**
   - Pause/resume
   - Cancel with rollback option
   - Retry failed operations
   - Skip problematic files

4. **Logs**
   - Real-time log viewer
   - Filter by severity
   - Export logs
   - Search functionality

**Deliverables**:
- `Views/MigrationExecutionPage.xaml`
- `ViewModels/MigrationExecutionViewModel.cs`
- Real-time event streaming

**Acceptance Criteria**:
- ? Responsive UI during migration
- ? Accurate progress reporting
- ? Easy pause/resume
- ? Complete audit trail

---

### 8.3 Validation View

**Objective**: Validate structure and naming with auto-fix options.

**Tasks**:

1. **Validation Configuration**
   - Select TIA blueprint
   - Select naming template
   - Choose folders to validate

2. **Results Display**
   - Issues by type
   - Severity indicators
   - Affected file list
   - Preview auto-fixes

3. **Auto-Fix**
   - Select issues to fix
   - Preview changes
   - Execute fixes
   - Undo support

**Deliverables**:
- `Views/ValidationPage.xaml`
- `ViewModels/ValidationViewModel.cs`
- Fix preview engine

**Acceptance Criteria**:
- ? Clear issue identification
- ? Safe auto-fix
- ? Preview before apply
- ? Undo capability

---

### 8.4 Settings & Configuration

**Objective**: Comprehensive settings management.

**Tasks**:

1. **Provider Configuration**
   - Add/remove providers
   - OAuth authentication
   - Sync folder paths
   - Rate limiting settings

2. **TIA Blueprint Editor**
   - Visual category editor
   - Drag-and-drop reordering
   - Import/export blueprints
   - Template gallery

3. **Naming Template Editor**
   - Live preview
   - Token palette
   - Validation
   - Template library

4. **Application Settings**
   - Theme selection
   - Language (i18n ready)
   - Log level
   - Telemetry opt-in/out
   - Database location

**Deliverables**:
- `Views/SettingsPage.xaml`
- Visual editors
- Configuration persistence

**Acceptance Criteria**:
- ? User-friendly editors
- ? Live validation
- ? Import/export configs
- ? Settings persistence

---

## Phase 9: Polish & Deployment (Weeks 17-18)

### 9.1 Installer & Packaging

**Objective**: Create installers for Windows and macOS.

**Tasks**:

1. **Windows Installer**
   - MSI via WiX Toolset
   - Start menu shortcuts
   - File association (.duconfig)
   - Uninstaller

2. **macOS Installer**
   - DMG package
   - Code signing
   - Notarization
   - Application bundle

3. **Auto-Update**
   - Check for updates
   - Download and install
   - Release notes display

**Deliverables**:
- Installer projects
- CI/CD pipeline (GitHub Actions)
- Release automation
- Update server

**Acceptance Criteria**:
- ? One-click installation
- ? Signed installers
- ? Auto-update works
- ? Clean uninstall

---

### 9.2 Documentation & Tutorials

**Objective**: Comprehensive user documentation.

**Tasks**:

1. **User Guide**
   - Getting started
   - Feature walkthroughs
   - Best practices
   - Troubleshooting

2. **Video Tutorials**
   - Quick start (5 min)
   - Assessment tutorial (10 min)
   - Migration tutorial (15 min)
   - Advanced features (20 min)

3. **In-App Help**
   - Tooltips
   - Context-sensitive help
   - Interactive tours

**Deliverables**:
- `docs/USER-GUIDE.md`
- Video tutorials
- In-app help system

**Acceptance Criteria**:
- ? Clear documentation
- ? Search functionality
- ? Up-to-date content

---

### 9.3 Telemetry & Diagnostics

**Objective**: Optional telemetry and diagnostics for support.

**Tasks**:

1. **Telemetry Framework**
   - Opt-in during onboarding
   - Anonymous usage metrics
   - Feature usage tracking
   - Performance metrics
   - Privacy-preserving

2. **Diagnostics**
   - Error reporting
   - Crash dumps
   - Log collection
   - System info gathering
   - Export diagnostics bundle

3. **Dashboard**
   - Usage analytics
   - Error trends
   - Performance insights

**Deliverables**:
- Telemetry infrastructure
- Privacy controls
- Diagnostics exporter
- Analytics dashboard

**Acceptance Criteria**:
- ? Opt-in by default off
- ? Clear privacy policy
- ? Easy to disable
- ? Useful for support

---

## Technical Architecture: GUI Details

### GUI Project Structure

```
DocsUnmessed.GUI/
??? App.xaml                    # Application entry point
??? AppShell.xaml               # Main navigation shell
??? MauiProgram.cs              # DI configuration
??? Views/
?   ??? DashboardPage.xaml
?   ??? AssessmentPage.xaml
?   ??? FileBrowserPage.xaml
?   ??? MigrationPlanningPage.xaml
?   ??? MigrationExecutionPage.xaml
?   ??? ValidationPage.xaml
?   ??? SettingsPage.xaml
??? ViewModels/
?   ??? BaseViewModel.cs
?   ??? DashboardViewModel.cs
?   ??? AssessmentViewModel.cs
?   ??? FileBrowserViewModel.cs
?   ??? MigrationPlanningViewModel.cs
?   ??? MigrationExecutionViewModel.cs
?   ??? ValidationViewModel.cs
?   ??? SettingsViewModel.cs
??? Services/
?   ??? NavigationService.cs
?   ??? DialogService.cs
?   ??? ProgressReporter.cs
??? Converters/
?   ??? BytesToHumanReadableConverter.cs
?   ??? IssueToColorConverter.cs
?   ??? BoolToVisibilityConverter.cs
??? Controls/
?   ??? FileTreeView.xaml
?   ??? ProgressCard.xaml
?   ??? KpiCard.xaml
??? Resources/
    ??? Styles/
    ??? Images/
    ??? Localization/
```

### MVVM Pattern

```csharp
// Base ViewModel
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// Example ViewModel
public class DashboardViewModel : BaseViewModel
{
    private long _totalFiles;
    public long TotalFiles
    {
        get => _totalFiles;
        set => SetProperty(ref _totalFiles, value);
    }
    
    private ICommand? _startAssessmentCommand;
    public ICommand StartAssessmentCommand => 
        _startAssessmentCommand ??= new AsyncRelayCommand(StartAssessmentAsync);
    
    private async Task StartAssessmentAsync()
    {
        await _navigationService.NavigateToAsync<AssessmentPage>();
    }
}
```

### Dependency Injection

```csharp
// MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Core services
        builder.Services.AddSingleton<IHashService, HashService>();
        builder.Services.AddSingleton<IInventoryService, SqliteInventoryService>();
        builder.Services.AddSingleton<IRulesEngine, RulesEngine>();
        builder.Services.AddSingleton<IMigrationOrchestrator, MigrationOrchestrator>();
        
        // Connectors
        builder.Services.AddTransient<IConnector, FileSystemConnector>();
        
        // Application services
        builder.Services.AddSingleton<IAssessmentService, AssessmentService>();
        builder.Services.AddSingleton<IMigrationService, MigrationService>();
        
        // GUI services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        
        // ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<AssessmentViewModel>();
        builder.Services.AddTransient<FileBrowserViewModel>();
        
        // Views
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<AssessmentPage>();
        builder.Services.AddTransient<FileBrowserPage>();

        return builder.Build();
    }
}
```

### Real-Time Updates

```csharp
// Progress reporting
public interface IProgressReporter
{
    IObservable<ProgressReport> Progress { get; }
    void Report(ProgressReport progress);
}

public class ProgressReporter : IProgressReporter
{
    private readonly Subject<ProgressReport> _progressSubject = new();
    
    public IObservable<ProgressReport> Progress => _progressSubject.AsObservable();
    
    public void Report(ProgressReport progress)
    {
        _progressSubject.OnNext(progress);
    }
}

// ViewModel subscription
public class AssessmentViewModel : BaseViewModel
{
    public AssessmentViewModel(IProgressReporter progressReporter)
    {
        progressReporter.Progress
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(OnProgressUpdated);
    }
    
    private void OnProgressUpdated(ProgressReport progress)
    {
        ItemsProcessed = progress.ItemsProcessed;
        CurrentFile = progress.CurrentFile;
        ProgressPercentage = progress.Percentage;
    }
}
```

---

## NuGet Packages Required

### Core Application
- `Microsoft.Data.Sqlite` - SQLite database
- `Microsoft.Extensions.DependencyInjection` - DI container
- `Microsoft.Extensions.Logging` - Logging framework

### Cloud Connectors
- `Microsoft.Graph` - OneDrive/SharePoint
- `Microsoft.Identity.Client` - OAuth for Microsoft
- `Google.Apis.Drive.v3` - Google Drive
- `Dropbox.Api` - Dropbox

### GUI (.NET MAUI)
- `CommunityToolkit.Maui` - Additional controls
- `CommunityToolkit.Mvvm` - MVVM helpers
- `LiveChartsCore.SkiaSharpView.Maui` - Charts
- `System.Reactive` - Reactive extensions

### Testing
- `xUnit` - Test framework
- `Moq` - Mocking
- `FluentAssertions` - Assertion library
- `Bogus` - Test data generation

---

## Success Metrics

### Performance
- ? Scan 100k files in <60 seconds
- ? UI responsive (<16ms frame time)
- ? Database queries <1 second
- ? Memory usage <500MB for 100k files

### Quality
- ? 80%+ code coverage
- ? Zero critical bugs
- ? <5 minor bugs per release
- ? User satisfaction >4.5/5

### Adoption
- ? 1000+ downloads in first month
- ? 50+ GitHub stars
- ? Active community support
- ? Contributing developers

---

## Risk Mitigation

### Technical Risks
- **Risk**: Cloud API rate limiting
  - **Mitigation**: Implement exponential backoff, batch operations
  
- **Risk**: Large file set performance
  - **Mitigation**: Database indexing, pagination, virtualization
  
- **Risk**: Cross-platform compatibility
  - **Mitigation**: Early testing on all platforms, CI/CD validation

### Business Risks
- **Risk**: User adoption
  - **Mitigation**: Strong documentation, video tutorials, community engagement
  
- **Risk**: Support burden
  - **Mitigation**: Comprehensive logs, diagnostics, FAQ, community forum

---

## Conclusion

This plan provides a comprehensive roadmap to mature DocsUnmessed from a CLI tool to a professional cross-platform application with an intuitive GUI. The phased approach ensures incremental value delivery while maintaining quality and extensibility.

**Estimated Timeline**: 16-18 weeks
**Estimated Effort**: 600-800 hours (1-2 developers)
**Risk Level**: Medium (cloud integrations, GUI complexity)
**ROI**: High (unique value proposition, personal productivity tool)

---

**Next Steps**:
1. Review and approve this plan
2. Set up project management (GitHub Projects/Jira)
3. Begin Phase 2: Core Feature Completion
4. Schedule weekly progress reviews
5. Establish CI/CD pipeline
6. Create feedback loop with early users

**Questions?** Review detailed architecture docs and reach out to the development team.

---

*Document Version: 1.0*
*Last Updated: 2025-01-03*
*Authors: DocsUnmessed Development Team*
