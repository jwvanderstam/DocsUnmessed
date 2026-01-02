# DocsUnmessed Project Standards

## Code Standards

### 1. Naming Conventions

**Classes**:
- PascalCase: `ScanEntity`, `RulesEngine`, `ItemEntity`
- Suffix entities with `Entity`: `ScanEntity`, `ItemEntity`
- Suffix interfaces with `I`: `IRule`, `IConnector`, `IInventoryService`
- Suffix services with `Service`: `HashService`, `InventoryService`

**Methods**:
- PascalCase: `ExecuteAsync`, `LoadRulesAsync`, `GetScanResultAsync`
- Async methods end with `Async`
- Boolean methods start with `Is`, `Has`, `Can`: `IsEnabled`, `HasValue`

**Properties**:
- PascalCase: `ScanId`, `ItemId`, `TotalFiles`
- Required properties marked with `required` keyword

**Fields**:
- Private fields: `_camelCase` with underscore prefix
- Example: `_orchestrator`, `_inventoryService`

**Parameters**:
- camelCase: `scanId`, `cancellationToken`, `options`

### 2. File Organization

**Directory Structure**:
```
src/
??? CLI/                    # Command-line interface
?   ??? Commands/          # Individual commands
??? Connectors/            # Storage provider connectors
??? Core/                  # Core domain logic
?   ??? Configuration/    # Configuration models
?   ??? Domain/           # Domain models
?   ??? Interfaces/       # Core interfaces
?   ??? Rules/            # Rule implementations
??? Data/                  # Data access layer
?   ??? Entities/         # EF Core entities
??? Services/              # Service implementations
```

**One Class Per File**:
- ? Each class in its own file
- ? File name matches class name: `ScanEntity.cs` contains `ScanEntity`

### 3. Namespace Conventions

**Pattern**: `DocsUnmessed.<Layer>[.<Category>]`

Examples:
- `DocsUnmessed.Core.Domain`
- `DocsUnmessed.Core.Interfaces`
- `DocsUnmessed.Core.Rules`
- `DocsUnmessed.Data.Entities`
- `DocsUnmessed.Services`
- `DocsUnmessed.CLI.Commands`

### 4. Code Quality

**XML Documentation**:
- ? All public classes have `<summary>` comments
- ? Complex methods documented
- ? Parameters documented when non-obvious

**Nullability**:
- ? Nullable reference types enabled
- ? Required properties marked with `required`
- ? Nullable properties marked with `?`

**Async/Await**:
- ? All I/O operations are async
- ? CancellationToken passed through
- ? Methods named with `Async` suffix

**LINQ**:
- ? Use LINQ for collections
- ? Prefer method syntax for simple queries
- ? Query syntax for complex joins

### 5. Entity Standards

**Data Annotations**:
```csharp
[Key]
[MaxLength(36)]
public required string EntityId { get; set; }

[Required]
[MaxLength(200)]
public required string Name { get; set; }

[MaxLength(2000)]
public string? OptionalField { get; set; }
```

**Navigation Properties**:
- Always initialize collections: `= new List<T>()`
- Foreign keys explicitly defined
- Both sides of relationship configured

**Timestamps**:
- `CreatedAt` and `UpdatedAt` on all primary entities
- Default to `DateTime.UtcNow`

### 6. Repository Pattern

**Interface Naming**:
- `IRepository<TEntity>` for generic
- `IScanRepository`, `IItemRepository` for specific

**Method Naming**:
- Get: `GetByIdAsync`, `GetAllAsync`
- Add: `AddAsync`, `AddRangeAsync`
- Update: `UpdateAsync`
- Delete: `DeleteAsync`, `DeleteRangeAsync`
- Query: `FindAsync`, `CountAsync`, `ExistsAsync`

---

## Documentation Standards

### 1. Code Documentation

**XML Comments**:
```csharp
/// <summary>
/// Entity representing a file system scan
/// </summary>
public sealed class ScanEntity
{
    /// <summary>
    /// Unique identifier for the scan
    /// </summary>
    public required string ScanId { get; set; }
}
```

**Inline Comments**:
- Use `//` for single-line comments
- Only when code is not self-explanatory
- Explain "why", not "what"

### 2. Documentation Files

**Markdown Standards**:
- Use `#` for titles (ATX style)
- Use tables for structured data
- Use code blocks with language hints: ```csharp
- Use ? ? ? for status indicators

**File Naming**:
- UPPERCASE-WITH-DASHES.md: `DATABASE-SCHEMA.md`
- Lower case for code: `appsettings.json`

**Documentation Structure**:
```
docs/
??? QUICK-START.md           # User getting started
??? FEATURE-SHOWCASE.md      # Feature documentation
??? DATABASE-SCHEMA.md       # Technical documentation
??? RULE-CONFIGURATION-FORMAT.md

planning/
??? ROADMAP.md               # Project planning
??? progress/                # Daily progress
??? reports/                 # Completion reports
```

### 3. Progress Documentation

**Daily Reports**: `Phase2-WeekX-DayY.md`
- Date and objective
- Completed tasks (?)
- Pending tasks (?)
- Metrics and statistics
- Next steps

**Day Summaries**: `DAYX-COMPLETE.md`
- Quick summary
- Key achievements
- Statistics table
- Links to detailed docs

---

## Solution Structure Standards

### 1. Project Organization

**Single Project** (current):
- All code in `DocsUnmessed.csproj`
- Logical separation via folders
- Clean namespace hierarchy

**Future Multi-Project**:
```
DocsUnmessed.sln
??? DocsUnmessed.Core/       # Domain + Interfaces
??? DocsUnmessed.Data/       # Data access
??? DocsUnmessed.Services/   # Services
??? DocsUnmessed.CLI/        # Command-line
??? DocsUnmessed.Tests/      # Tests
```

### 2. Configuration

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=docsunmessed.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Environment-specific**:
- `appsettings.Development.json`
- `appsettings.Production.json`
- Local settings in `.gitignore`

### 3. Dependencies

**Package Management**:
- Use specific versions, not wildcards
- Document why packages are needed
- Keep up-to-date with security patches

**Current Packages**:
- Microsoft.EntityFrameworkCore.Sqlite: 9.0.0
- Microsoft.EntityFrameworkCore.Design: 9.0.0
- (Future: xUnit, FluentAssertions when .NET 10 compatible)

---

## Testing Standards

### 1. Test Organization

**Structure**:
```
tests/
??? DocsUnmessed.Tests.Unit/
?   ??? Rules/
?   ??? Services/
?   ??? Helpers/
??? DocsUnmessed.Tests.Integration/
    ??? Data/
    ??? Connectors/
```

**Naming**:
- Test classes: `ClassNameTests`
- Test methods: `MethodName_Scenario_ExpectedResult`
- Example: `EvaluateAsync_ValidItem_ReturnsSuggestion`

### 2. Test Quality

**Arrange-Act-Assert**:
```csharp
[Fact]
public async Task GetByIdAsync_ExistingScan_ReturnsScan()
{
    // Arrange
    var repository = new ScanRepository(context);
    var scanId = "test-id";
    
    // Act
    var result = await repository.GetByIdAsync(scanId);
    
    // Assert
    result.Should().NotBeNull();
    result.ScanId.Should().Be(scanId);
}
```

**Coverage Goals**:
- Core logic: 90%+
- Services: 80%+
- UI/CLI: 60%+
- Overall: 80%+

---

## Git Standards

### 1. Commit Messages

**Format**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `refactor`: Code refactoring
- `test`: Tests
- `chore`: Build/tools

**Examples**:
```
feat(data): add Entity Framework Core entities
docs(database): create database schema documentation
refactor(rules): improve performance with compiled regex
```

### 2. Branch Strategy

**Main Branch**: `main`
- Production-ready code
- Protected, requires PR

**Feature Branches**: `feature/description`
- `feature/sqlite-persistence`
- `feature/rules-engine`

**Fix Branches**: `fix/issue-description`
- `fix/validation-error`

### 3. .gitignore

**Excluded**:
- `bin/`, `obj/` - Build outputs
- `*.user` - User-specific files
- `logs/*.log` - Log files
- `*.db` - Database files
- `.vs/`, `.vscode/` - IDE folders

---

## Performance Standards

### 1. Database

**Query Optimization**:
- Use indexes on foreign keys
- Pagination for large result sets
- AsNoTracking for read-only queries
- Compiled queries for hot paths

**Targets**:
- Simple queries: <50ms
- Complex queries: <100ms
- Batch operations: <1s per 100 items

### 2. Code

**Best Practices**:
- Use `StringBuilder` for string concatenation
- Compile regex patterns
- Use `ValueTask` for hot paths
- Avoid unnecessary allocations

---

## Security Standards

### 1. Data Protection

**Sensitive Data**:
- Connection strings in configuration
- OAuth tokens in OS keychain
- No secrets in code or git

**Validation**:
- Input validation on all public APIs
- SQL parameterization (EF Core handles)
- Path traversal prevention

### 2. Audit Trail

**Logging**:
- All operations in AuditLog table
- Timestamp, action, entity, success
- Error messages without sensitive data

---

## Compliance Checklist

### Code Quality
- [ ] XML documentation on public APIs
- [ ] Nullable reference types enabled
- [ ] Async/await for I/O operations
- [ ] CancellationToken support
- [ ] No compiler warnings

### Structure
- [ ] One class per file
- [ ] Correct namespace hierarchy
- [ ] Proper folder organization
- [ ] Consistent naming conventions

### Documentation
- [ ] User documentation up-to-date
- [ ] Technical documentation complete
- [ ] Progress tracked daily
- [ ] Standards documented

### Testing
- [ ] Unit tests for core logic
- [ ] Integration tests for data
- [ ] Test coverage >80%
- [ ] All tests passing

---

*Last Updated: January 3, 2025*  
*Version: 1.0*  
*Status: Living Document*
