# DocsUnmessed Rules Engine - Feature Showcase

## Overview

The Rules Engine is a powerful, flexible system for automatically organizing files based on customizable rules. It supports multiple rule types, priority-based evaluation, and provides detailed suggestions with confidence scores.

---

## ?? Core Features

### 1. Multiple Rule Types

#### **RegexPathRule** - Pattern Matching
Match files based on path patterns using regular expressions.

```json
{
  "name": "Downloads-Screenshots",
  "match": {
    "pathRegex": "(?i)Downloads.*screenshot"
  },
  "target": {
    "location": "OneDrive://04_Media/Screenshots/"
  },
  "conflictPolicy": "TimestampSuffix",
  "priority": 200
}
```

**Features**:
- Case-insensitive matching
- Full .NET regex support
- Compiled regex for performance
- 95% confidence scoring

**Use Cases**:
- Match files by folder location
- Match by filename patterns
- Match by keywords in path

---

#### **ExtensionRule** - File Type Organization
Match files by extension for type-based organization.

```json
{
  "name": "Office-Documents",
  "match": {
    "extensions": ["docx", "xlsx", "pptx", "pdf"]
  },
  "target": {
    "location": "OneDrive://02_Work/Documents/"
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 150
}
```

**Features**:
- Multiple extensions per rule
- Case-insensitive matching
- No dots required in extensions
- 90% confidence scoring

**Use Cases**:
- Organize by file type (images, documents, code)
- Group related file formats
- Categorize by application

---

#### **AgeBasedRule** - Time-Based Organization
Match files based on age for archival or cleanup.

```json
{
  "name": "Archive-Old-Files",
  "match": {
    "extensions": ["pdf", "docx"],
    "ageDaysMin": 365
  },
  "target": {
    "location": "OneDrive://99_Archive/"
  },
  "conflictPolicy": "TimestampSuffix",
  "priority": 100
}
```

**Features**:
- Min and max age constraints
- Age ranges (between X and Y days)
- Based on last modified date
- 85% confidence scoring

**Use Cases**:
- Archive old files
- Organize recent documents
- Cleanup temporary files
- Lifecycle management

---

#### **CompositeRule** - Complex Conditions
Combine multiple criteria with AND/OR logic.

```json
{
  "name": "Old-Work-PDFs",
  "match": {
    "pathRegex": "(?i)work|project",
    "extensions": ["pdf"],
    "ageDaysMin": 90
  },
  "target": {
    "location": "OneDrive://02_Work/Archive/"
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 300
}
```

**Features**:
- AND logic (all criteria must match)
- Combines regex, extensions, and age
- Aggregates reasons from all criteria
- 88% confidence scoring

**Use Cases**:
- Specific file type in specific location with age constraint
- Complex business rules
- Multi-criteria filtering

---

### 2. Priority-Based Evaluation

Rules are evaluated in priority order (highest first).

**Priority Guidelines**:
```
500+     Critical/Urgent files
400-499  High priority (financial, work)
300-399  Medium-high (active projects)
200-299  Medium (standard organization)
100-199  Low (archives, fallback)
1-99     Very low (catch-all)
```

**How It Works**:
1. Rules sorted by priority (descending)
2. First matching rule wins
3. Clear, predictable behavior
4. Easy to override with higher priority

**Example**:
```json
[
  {
    "name": "Recent-Financial",
    "priority": 400,
    "match": { "pathRegex": "(?i)invoice", "ageDaysMax": 30 }
  },
  {
    "name": "All-Financial",
    "priority": 350,
    "match": { "pathRegex": "(?i)invoice" }
  }
]
```
Result: Recent invoices go to high-priority rule, older ones to lower priority.

---

### 3. Confidence Scoring

Every suggestion includes a confidence score (0.0 to 1.0).

**Confidence Levels**:
- **0.95** - RegexPathRule (very specific)
- **0.90** - ExtensionRule (type-based)
- **0.88** - CompositeRule (multi-criteria)
- **0.85** - AgeBasedRule (time-based)

**Use Cases**:
- Filter low-confidence suggestions
- Review high-confidence auto-migrations
- Build trust in automation

---

### 4. Reason Explanations

Every suggestion explains WHY it matched.

**Example Output**:
```json
{
  "targetPath": "OneDrive://02_Work/Archive/",
  "targetName": "report.pdf",
  "confidence": 0.95,
  "reasons": [
    "Matched regex pattern: (?i)work",
    "Matched extension: .pdf",
    "File age: 120 days (older than minimum: 90 days)"
  ]
}
```

**Benefits**:
- Transparency in decision-making
- Easy to debug rules
- Build user confidence
- Audit trail

---

### 5. Conflict Resolution Policies

Four strategies for handling naming conflicts:

#### **VersionSuffix** (Recommended)
```
document.pdf ? document (2).pdf
document (2).pdf ? document (3).pdf
```

#### **TimestampSuffix** (Archives)
```
document.pdf ? document_20250103_143022.pdf
```

#### **Skip** (Safe)
```
If target exists, don't move
```

#### **Overwrite** (Dangerous)
```
Replace existing file (use with caution)
```

---

### 6. Comprehensive Statistics

**Output Includes**:
- Total files scanned
- Files with suggestions
- Files without suggestions
- Coverage percentage
- Grouping by target location
- Grouping by rule name
- Sample suggestions (first 10)

**Example Output**:
```
Rules Evaluation Results:
  Files with suggestions: 65
  Files without suggestions: 35
  Coverage: 65.0%

Suggestions by Target Location:
  OneDrive://04_Media/Photos/: 25 files
  OneDrive://03_Tech/Archive/: 20 files
  OneDrive://02_Work/Current/: 20 files

Suggestions by Rule:
  Photos-Organization: 25 files
  Tech-Archive: 20 files
  Work-Documents: 20 files
```

---

### 7. JSON Export

Export complete suggestions to JSON for:
- Review before execution
- Integration with other tools
- Audit trails
- Reporting

**Export Format**:
```json
{
  "scanId": "abc123",
  "generatedAt": "2025-01-03T10:30:00Z",
  "rulesPath": "examples/mapping-rules.json",
  "summary": {
    "totalFiles": 100,
    "filesWithSuggestions": 65,
    "filesWithoutSuggestions": 35,
    "coveragePercent": 65.0
  },
  "suggestions": [...]
}
```

---

### 8. Rule Validation

Validate rules before using them with 15+ checks:

**Validation Checks**:
- ? Required fields
- ? Regex syntax
- ? Extension format
- ? Age constraint consistency
- ? Conflict policy validity
- ? Priority range
- ? Duplicate detection
- ? Convention recommendations

**Example**:
```bash
$ dotnet run -- validate --rules my-rules.json

? Rule validation passed with no issues
```

Or with errors:
```bash
? Validation Failed (3 error(s)):
  ERROR: Rule #1: Invalid regex pattern
  ERROR: Rule #2: No match criteria specified
  ERROR: Rule #3: Negative age value

? Warnings (2):
  WARNING: AgeDaysMin very large (3650 days)
  WARNING: Multiple rules with priority 100
```

---

## ?? Usage Examples

### Example 1: Basic File Organization
```bash
# 1. Scan files
dotnet run -- assess --providers fs_local --root C:\MyFiles --out scan.json

# 2. Validate rules
dotnet run -- validate --rules examples/mapping-rules.json

# 3. Simulate organization
dotnet run -- simulate --scan-id <id> --rules examples/mapping-rules.json --out suggestions.json

# 4. Review suggestions
cat suggestions.json | jq '.summary'

# 5. Execute (when ready)
dotnet run -- migrate --plan suggestions.json
```

---

### Example 2: Media Organization
Organize photos, videos, and music by type and age.

```bash
dotnet run -- simulate --scan-id <id> \
  --rules examples/mapping-rule-media.json \
  --out media-org.json
```

**Results**:
- RAW photos ? OneDrive://04_Media/Photos/RAW/
- Videos ? OneDrive://04_Media/Videos/
- Audio ? OneDrive://04_Media/Audio/
- Old media ? OneDrive://04_Media/Archive/

---

### Example 3: Developer Workspace
Organize source code, configs, and documentation.

```bash
dotnet run -- simulate --scan-id <id> \
  --rules examples/mapping-rule-development.json \
  --out dev-org.json
```

**Results**:
- Active code (<90 days) ? OneDrive://03_Tech/Development/Active/
- Old code (>90 days) ? OneDrive://03_Tech/Development/Archive/
- Config files ? OneDrive://03_Tech/Configurations/
- Scripts ? OneDrive://03_Tech/Scripts/
- Documentation ? OneDrive://03_Tech/Documentation/

---

### Example 4: Work Documents
Organize work documents by age and type.

```bash
dotnet run -- simulate --scan-id <id> \
  --rules examples/mapping-rule-work.json \
  --out work-org.json
```

**Results**:
- Current documents (<90 days) ? OneDrive://02_Work/Current/
- Archive (>90 days) ? OneDrive://02_Work/Archive/
- Meeting notes ? OneDrive://02_Work/Meetings/
- Presentations ? OneDrive://02_Work/Presentations/
- Data/analysis ? OneDrive://02_Work/Data/

---

### Example 5: Archive Old Files
Clean up old files by archiving them.

```bash
dotnet run -- simulate --scan-id <id> \
  --rules examples/mapping-rule-archive.json \
  --out archive.json
```

**Results**:
- Old office docs (>180 days) ? OneDrive://03_Tech/99_Archive/Office/
- Old PDFs ? OneDrive://01_Personal/Reference/Manuals/
- Financial archives ? OneDrive://01_Personal/Finance/Archive/
- Code archives ? OneDrive://03_Tech/Archives/

---

## ?? Advanced Features

### Custom Rule Creation

**Template-Based**:
Use templates from `examples/mapping-rule-templates.json`:
1. Copy a template
2. Modify name, criteria, and target
3. Validate with CLI
4. Use in simulation

**From Scratch**:
```json
{
  "name": "My-Custom-Rule",
  "match": {
    "pathRegex": "(?i)pattern",
    "extensions": ["ext1", "ext2"],
    "ageDaysMin": 30,
    "ageDaysMax": 180
  },
  "target": {
    "location": "OneDrive://Target/",
    "namingTemplate": null
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 200
}
```

---

### Rule Composition

Combine simple rules into complex ones:

```json
{
  "name": "Important-Recent-Work-Docs",
  "match": {
    "pathRegex": "(?i)(important|urgent|critical)",
    "extensions": ["docx", "xlsx", "pdf"],
    "ageDaysMax": 30
  },
  "target": {
    "location": "OneDrive://01_Personal/Important/"
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 500
}
```

---

### Priority Strategies

**Strategy 1: Specific Before General**
```json
[
  { "name": "Work-PDFs-Recent", "priority": 400 },
  { "name": "Work-PDFs-All", "priority": 350 },
  { "name": "All-PDFs", "priority": 200 }
]
```

**Strategy 2: Important Before Routine**
```json
[
  { "name": "Critical-Files", "priority": 500 },
  { "name": "High-Priority", "priority": 400 },
  { "name": "Normal-Files", "priority": 200 }
]
```

**Strategy 3: Recent Before Old**
```json
[
  { "name": "Recent-Work", "priority": 350 },
  { "name": "Old-Work", "priority": 300 }
]
```

---

## ?? Performance

### Efficiency
- **Compiled Regex**: Fast pattern matching
- **Async Operations**: Non-blocking I/O
- **LINQ Optimization**: Efficient grouping
- **Lazy Evaluation**: Process only what's needed

### Expected Performance
- **Loading Rules**: <100ms
- **Evaluating 100 Files**: <1 second
- **Export to JSON**: <100ms
- **Total End-to-End**: <5 seconds

---

## ?? Best Practices

### Rule Design
1. **Start Specific**: Higher priority for specific rules
2. **Test Regex**: Use regex101.com to validate patterns
3. **Use Templates**: Copy from examples/mapping-rule-templates.json
4. **Document Purpose**: Clear rule names

### Configuration
1. **Validate First**: Always run validate before simulate
2. **Start Small**: Begin with few rules, expand gradually
3. **Review Output**: Check suggestions before executing
4. **Use Versioning**: Keep rule configs in version control

### Testing Workflow
1. Create/edit rules
2. Validate with CLI
3. Fix errors, re-validate
4. Simulate on test data
5. Review suggestions
6. Execute on real data

---

## ?? Documentation

**Complete Documentation Available**:
- `docs/RULE-CONFIGURATION-FORMAT.md` - Complete reference
- `docs/MANUAL-TEST-PLAN.md` - Testing scenarios
- `docs/WEEK1-COMPLETION-REPORT.md` - Implementation details
- `examples/` - 28 production-ready rules

---

## ?? Summary

The Rules Engine provides:
- ? 4 flexible rule types
- ? Priority-based evaluation
- ? Confidence scoring
- ? Reason explanations
- ? Multiple conflict policies
- ? Comprehensive statistics
- ? JSON export
- ? Rule validation
- ? 28 example rules
- ? Extensive documentation

**Ready for production use with .NET 10 RTM.**

---

*DocsUnmessed Rules Engine Feature Showcase*  
*Version 1.0*  
*Date: January 3, 2025*
