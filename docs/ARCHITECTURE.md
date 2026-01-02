# DocsUnmessed Architecture

## Overview

DocsUnmessed follows a layered architecture with clear separation of concerns:

```
???????????????????????????????????????
?         CLI / UI Layer              ?
?  (Command routing, user interface)  ?
???????????????????????????????????????
              ?
???????????????????????????????????????
?       Application Services          ?
?  (Orchestration, business logic)    ?
???????????????????????????????????????
              ?
???????????????????????????????????????
?       Core Domain & Interfaces      ?
?  (Models, contracts, abstractions)  ?
???????????????????????????????????????
              ?
???????????????????????????????????????
?    Infrastructure / Connectors      ?
?  (Storage providers, file system)   ?
???????????????????????????????????????
```

## Core Components

### 1. Domain Layer (`src/Core/Domain/`)

**Purpose**: Define core business entities and value objects.

**Key Models**:
- `Item`: Represents a file or folder with metadata
- `Operation`: Represents a file operation (copy, move, rename)
- `ScanResult`: Aggregates scan inventory and statistics
- `TargetSuggestion`: Represents a recommended target location

**Design Principles**:
- Immutable record types
- Rich domain models with validation
- No dependencies on infrastructure

### 2. Interface Layer (`src/Core/Interfaces/`)

**Purpose**: Define contracts for core services.

**Key Interfaces**:
- `IConnector`: Abstraction for storage providers
- `IInventoryService`: Manages file inventory and scans
- `IRulesEngine`: Evaluates mapping rules
- `IMigrationOrchestrator`: Orchestrates migration operations

**Design Principles**:
- Interface segregation
- Async/await patterns
- Cancellation token support

### 3. Configuration Layer (`src/Core/Configuration/`)

**Purpose**: Define configuration models for blueprints, templates, and rules.

**Key Models**:
- `TiaBlueprint`: Target Information Architecture
- `NamingTemplate`: File naming conventions
- `MappingRule`: File-to-location mapping rules

### 4. Connector Layer (`src/Connectors/`)

**Purpose**: Implement storage provider abstractions.

**Current Implementations**:
- `FileSystemConnector`: Local file system and network shares

**Planned**:
- `OneDriveConnector`: OneDrive Personal/Business
- `GoogleDriveConnector`: Google Drive
- `DropboxConnector`: Dropbox

**Design Principles**:
- Plugin architecture
- Support for both API and local sync folder modes
- Rate limiting and retry logic

### 5. Service Layer (`src/Services/`)

**Purpose**: Implement core business services.

**Key Services**:
- `InMemoryInventoryService`: Manages scan inventory (in-memory)
- `HashService`: Computes SHA-256 file hashes
- `PathValidator`: Validates and sanitizes paths
- `MigrationOrchestrator`: Coordinates migration operations

**Design Principles**:
- Single responsibility
- Dependency injection ready
- Testable implementations

### 6. CLI Layer (`src/CLI/`)

**Purpose**: Provide command-line interface.

**Components**:
- `CommandRouter`: Routes commands to handlers
- `Commands/`: Individual command implementations
  - `AssessCommand`: Scan and assess storage
  - `SimulateCommand`: What-if migration planning

**Design Principles**:
- Command pattern
- Argument parsing and validation
- Progress reporting

## Data Flow

### Assessment Flow

```
User Command
    ?
CommandRouter
    ?
AssessCommand
    ?
IConnector.EnumerateAsync() ? Items
    ?
IInventoryService.AddItemsAsync() ? Stored
    ?
ScanResult ? Export (JSON/CSV)
```

### Migration Flow (Planned)

```
User Command
    ?
SimulateCommand
    ?
IMigrationOrchestrator.CreatePlanAsync()
    ?
IRulesEngine.EvaluateAsync() ? TargetSuggestions
    ?
MigrationPlan ? Export or Execute
    ?
IConnector.OperateAsync() ? File Operations
    ?
AuditLog + Results
```

## Key Design Patterns

### 1. Strategy Pattern
Used in `IConnector` to support different storage providers with the same interface.

### 2. Repository Pattern
`IInventoryService` acts as a repository for scan data and file inventory.

### 3. Command Pattern
CLI commands encapsulate operations with execute methods.

### 4. Builder Pattern (Planned)
For constructing complex migration plans and configurations.

### 5. Observer Pattern (Planned)
For progress reporting during long-running operations.

## Extensibility Points

### Adding a New Connector

1. Implement `IConnector` interface
2. Handle authentication (if API-based)
3. Implement `EnumerateAsync()` for scanning
4. Implement `OperateAsync()` for file operations
5. Define `ProviderLimits` for constraints
6. Register in DI container

Example:
```csharp
public class OneDriveConnector : IConnector
{
    public string Id => "onedrive_api";
    public ConnectorMode Mode => ConnectorMode.Api;
    
    // Implementation...
}
```

### Adding a New Command

1. Create command class in `src/CLI/Commands/`
2. Implement execute method
3. Register in `CommandRouter`
4. Add argument parsing logic

Example:
```csharp
public sealed class ValidateCommand
{
    public async Task<int> ExecuteAsync(
        ValidateOptions options, 
        CancellationToken cancellationToken)
    {
        // Implementation...
    }
}
```

### Adding a New Rule Type

1. Implement `IRule` interface
2. Define match predicate
3. Define mapping logic
4. Register in rules engine

## Performance Considerations

### Scanning
- Parallel enumeration with thread pooling
- Configurable hash computation (skip for large files)
- Incremental scanning with caching
- Memory-efficient streaming

### Storage
- In-memory for development (current)
- SQLite for production (planned)
- Indexed queries for fast lookups
- Batch inserts for performance

### Migration
- Batched operations (configurable size)
- Checkpointing for resumability
- Parallel execution with rate limiting
- Hash verification for integrity

## Security Model

### Authentication
- OAuth tokens stored in OS keychain
- No password storage
- Optional local-only mode (no API access)

### Privacy
- All processing local
- No telemetry by default
- Optional metadata extraction
- Audit logging for transparency

### File Operations
- Non-destructive by default
- Copy-verify-delete strategy
- Quarantine for errors
- Reversibility support

## Testing Strategy

### Unit Tests (Planned)
- Domain model validation
- Service logic testing
- Mock connectors
- Rule evaluation

### Integration Tests (Planned)
- End-to-end command execution
- Real file system operations
- Mock API providers
- Performance benchmarks

### Test Structure
```
tests/
??? DocsUnmessed.Tests.Unit/
?   ??? Domain/
?   ??? Services/
?   ??? CLI/
??? DocsUnmessed.Tests.Integration/
    ??? Connectors/
    ??? EndToEnd/
```

## Future Enhancements

### Phase 2
- SQLite persistence
- Full rules engine
- Migration execution
- Duplicate handling

### Phase 3
- Cloud provider connectors
- OAuth flows
- API rate limiting
- Sync conflict detection

### Phase 4
- Desktop UI
- Real-time monitoring
- Scheduled migrations
- Advanced analytics

## References

- [PRD Document](../PRD.md)
- [API Documentation](./API.md) (coming soon)
- [Contributor Guide](../CONTRIBUTING.md) (coming soon)
