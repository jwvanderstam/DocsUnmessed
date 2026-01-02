# Rule Configuration Format Documentation

## Overview

DocsUnmessed uses JSON-based rule configurations to determine how files should be organized. Rules are evaluated in priority order, and the first matching rule determines the target location for a file.

## Table of Contents

1. [Rule Structure](#rule-structure)
2. [Match Criteria](#match-criteria)
3. [Target Configuration](#target-configuration)
4. [Conflict Policies](#conflict-policies)
5. [Priority System](#priority-system)
6. [Examples](#examples)
7. [Best Practices](#best-practices)
8. [Validation](#validation)

---

## Rule Structure

A rule configuration file is a JSON array of rule objects. Each rule object has the following structure:

```json
{
  "name": "string (required)",
  "match": {
    "pathRegex": "string (optional)",
    "extensions": ["string", ...] (optional)",
    "ageDaysMin": number (optional),
    "ageDaysMax": number (optional)
  },
  "target": {
    "location": "string (required)",
    "namingTemplate": "string or null (optional)"
  },
  "conflictPolicy": "string (required)",
  "priority": number (required)
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Unique identifier for the rule |
| `match` | object | Yes | Criteria for matching files |
| `target` | object | Yes | Destination configuration |
| `conflictPolicy` | string | Yes | How to handle naming conflicts |
| `priority` | number | Yes | Rule evaluation order (higher = evaluated first) |

---

## Match Criteria

The `match` object defines which files the rule applies to. At least one match criterion must be specified.

### Available Criteria

#### 1. `pathRegex` (string, optional)
Regular expression pattern to match against the full file path.

**Features:**
- Case-insensitive by default (uses `(?i)` flag)
- Supports standard .NET regex syntax
- Matches anywhere in the path

**Examples:**
```json
"pathRegex": "(?i)(Downloads/)"          // Matches files in Downloads folder
"pathRegex": "(?i)(invoice|receipt)"     // Matches files with "invoice" or "receipt" in path
"pathRegex": "\\d{4}-\\d{2}-\\d{2}"      // Matches dates like 2025-01-03
```

#### 2. `extensions` (string array, optional)
List of file extensions to match (without the dot).

**Features:**
- Case-insensitive matching
- Matches exact extensions only
- Multiple extensions supported

**Examples:**
```json
"extensions": ["pdf"]                                    // PDF files only
"extensions": ["jpg", "png", "gif"]                      // Image files
"extensions": ["doc", "docx", "xls", "xlsx", "ppt", "pptx"]  // Office files
```

#### 3. `ageDaysMin` (number, optional)
Minimum file age in days (based on last modified date).

**Examples:**
```json
"ageDaysMin": 90    // Files older than 90 days
"ageDaysMin": 365   // Files older than 1 year
```

#### 4. `ageDaysMax` (number, optional)
Maximum file age in days (based on last modified date).

**Examples:**
```json
"ageDaysMax": 30    // Files newer than 30 days
"ageDaysMax": 90    // Files newer than 90 days
```

### Composite Matching

When multiple criteria are specified, **ALL criteria must match** (AND logic).

**Example:**
```json
{
  "match": {
    "pathRegex": "(?i)(Downloads/)",
    "extensions": ["pdf"],
    "ageDaysMin": 90
  }
}
```
This matches: PDF files in Downloads folder that are older than 90 days.

---

## Target Configuration

The `target` object specifies where matched files should be moved.

### Fields

#### `location` (string, required)
Destination path for the file.

**Format:** `Provider://Path/To/Folder/`

**Providers:**
- `OneDrive://` - OneDrive for Business
- `SharePoint://` - SharePoint Online
- `Local://` - Local file system
- `Network://` - Network share

**Examples:**
```json
"location": "OneDrive://01_Personal/Documents/"
"location": "OneDrive://02_Work/Archive/"
"location": "OneDrive://03_Tech/Development/"
"location": "OneDrive://04_Media/Photos/"
```

#### `namingTemplate` (string or null, optional)
Template for renaming files. Use `null` to keep original name.

**Available Templates:**
- `"StandardDateContextTitle"` - Date-based naming with context
- `null` - Keep original filename

**Examples:**
```json
"namingTemplate": null                        // Keep: "document.pdf"
"namingTemplate": "StandardDateContextTitle"  // Rename to: "2025-01-03_Context_Title.pdf"
```

---

## Conflict Policies

Defines how to handle naming conflicts when the target file already exists.

### Available Policies

| Policy | Behavior | Example |
|--------|----------|---------|
| `VersionSuffix` | Add version number | `document (2).pdf` |
| `TimestampSuffix` | Add timestamp | `document_20250103_143022.pdf` |
| `Overwrite` | Replace existing file | `document.pdf` (overwrites) |
| `Skip` | Do not move if conflict | (file stays in source) |

**Examples:**
```json
"conflictPolicy": "VersionSuffix"     // Most common
"conflictPolicy": "TimestampSuffix"   // Best for archives
"conflictPolicy": "Skip"              // Safe option
```

---

## Priority System

Rules are evaluated in descending priority order (highest first).

### Priority Guidelines

| Range | Purpose | Example Use Cases |
|-------|---------|-------------------|
| 500+ | Critical/Urgent | Important documents, time-sensitive files |
| 400-499 | High Priority | Financial documents, work files |
| 300-399 | Medium-High | Recent projects, active work |
| 200-299 | Medium | Standard organization rules |
| 100-199 | Low | Archive rules, fallback rules |
| 1-99 | Very Low | Catch-all rules |

### Priority Best Practices

1. **Specific Before General**: Higher priority for more specific rules
2. **Recent Before Old**: Higher priority for recent file rules
3. **Important Before Routine**: Higher priority for critical content
4. **Composite Before Simple**: Higher priority for multi-criteria rules

**Example Priority Order:**
```json
[
  { "priority": 400, "name": "Financial-Recent" },      // Most specific
  { "priority": 350, "name": "Financial-Archive" },     // Less specific
  { "priority": 200, "name": "Photos-All" },            // General
  { "priority": 100, "name": "Old-Files-Archive" }      // Catch-all
]
```

---

## Examples

### Example 1: Simple Extension Rule
```json
{
  "name": "PDF-Documents",
  "match": {
    "extensions": ["pdf"]
  },
  "target": {
    "location": "OneDrive://01_Personal/Documents/",
    "namingTemplate": null
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 100
}
```

### Example 2: Path Pattern Rule
```json
{
  "name": "Downloads-Screenshots",
  "match": {
    "pathRegex": "(?i)(Downloads/).*screenshot",
    "extensions": ["png", "jpg"]
  },
  "target": {
    "location": "OneDrive://04_Media/Screenshots/",
    "namingTemplate": null
  },
  "conflictPolicy": "TimestampSuffix",
  "priority": 200
}
```

### Example 3: Age-Based Archive Rule
```json
{
  "name": "Archive-Old-Files",
  "match": {
    "extensions": ["pdf", "docx", "xlsx"],
    "ageDaysMin": 365
  },
  "target": {
    "location": "OneDrive://99_Archive/",
    "namingTemplate": "StandardDateContextTitle"
  },
  "conflictPolicy": "TimestampSuffix",
  "priority": 150
}
```

### Example 4: Composite Rule (Multiple Criteria)
```json
{
  "name": "Financial-Documents-Recent",
  "match": {
    "pathRegex": "(?i)(invoice|receipt|finance)",
    "extensions": ["pdf", "xlsx"],
    "ageDaysMax": 365
  },
  "target": {
    "location": "OneDrive://01_Personal/Finance/Recent/",
    "namingTemplate": "StandardDateContextTitle"
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 400
}
```

### Example 5: Work Documents Rule
```json
{
  "name": "Work-Current-Projects",
  "match": {
    "pathRegex": "(?i)(work|project|client)",
    "extensions": ["docx", "xlsx", "pptx"],
    "ageDaysMax": 90
  },
  "target": {
    "location": "OneDrive://02_Work/Current/",
    "namingTemplate": "StandardDateContextTitle"
  },
  "conflictPolicy": "VersionSuffix",
  "priority": 350
}
```

---

## Best Practices

### 1. Rule Naming
- Use descriptive, clear names
- Include category or type in name
- Format: `Category-Criteria-Action`
- Examples: `Photos-RAW-Archive`, `Work-Recent-Documents`

### 2. Match Criteria
- **Start specific, then generalize**: Use composite rules for specific cases
- **Test regex patterns**: Use tools like regex101.com
- **Use age ranges carefully**: Consider file lifecycle
- **List common extensions first**: More efficient matching

### 3. Target Organization
- **Use consistent hierarchy**: `Category/Subcategory/Type/`
- **Follow TIA structure**: Personal, Work, Tech, Media
- **Keep depth reasonable**: 3-5 levels maximum
- **Use meaningful names**: Descriptive folder names

### 4. Priority Assignment
- **Reserve 500+**: For critical/urgent files only
- **Use ranges**: Group related rules by priority range
- **Leave gaps**: Allow for future rule insertion
- **Document rationale**: Comment why certain priorities chosen

### 5. Conflict Policies
- **VersionSuffix**: Default for most cases
- **TimestampSuffix**: Archives and historical data
- **Skip**: When source is authoritative
- **Avoid Overwrite**: Use with caution

### 6. Testing
- **Start with small sets**: Test rules on limited files
- **Use what-if mode**: Simulate before actual migration
- **Review suggestions**: Check output before executing
- **Iterate**: Refine rules based on results

---

## Validation

### Required Fields
- Every rule must have: `name`, `match`, `target`, `conflictPolicy`, `priority`
- `match` must have at least one criterion
- `target.location` is required
- `priority` must be a positive integer

### Common Errors

**Error: No Match Criteria**
```json
// ? Invalid - empty match
{
  "match": {}
}

// ? Valid - at least one criterion
{
  "match": {
    "extensions": ["pdf"]
  }
}
```

**Error: Invalid Regex**
```json
// ? Invalid - unclosed bracket
{
  "pathRegex": "([incomplete"
}

// ? Valid - proper regex
{
  "pathRegex": "(?i)(Downloads/)"
}
```

**Error: Missing Target Location**
```json
// ? Invalid - no location
{
  "target": {
    "namingTemplate": "StandardDateContextTitle"
  }
}

// ? Valid - location specified
{
  "target": {
    "location": "OneDrive://Archive/",
    "namingTemplate": "StandardDateContextTitle"
  }
}
```

---

## Advanced Topics

### Regex Tips
- Use `(?i)` for case-insensitive matching
- Escape special characters: `\\.` for literal dot
- Use `.*` for wildcard matching
- Group alternatives: `(option1|option2|option3)`
- Test patterns before deploying

### Age Calculation
- Based on `ModifiedUtc` (last modified date)
- Calculated at evaluation time
- Use ranges for lifecycle stages:
  - 0-30 days: Current/Recent
  - 30-90 days: Active
  - 90-365 days: Semi-active
  - 365+ days: Archive

### Priority Conflicts
When multiple rules match:
1. Highest priority wins
2. If priorities equal, first rule wins
3. Consider making specific rules higher priority

---

## Rule Configuration Templates

See `examples/mapping-rule-templates.json` for copy-paste templates.

### Quick Start Template
```json
[
  {
    "name": "My-First-Rule",
    "match": {
      "extensions": ["pdf"]
    },
    "target": {
      "location": "OneDrive://Documents/",
      "namingTemplate": null
    },
    "conflictPolicy": "VersionSuffix",
    "priority": 100
  }
]
```

---

## File Organization

### Example Rule Files
- `mapping-rules.json` - Main rules (general use)
- `mapping-rule-archive.json` - Archive-specific rules
- `mapping-rule-media.json` - Photos, videos, audio
- `mapping-rule-development.json` - Code and technical files
- `mapping-rule-work.json` - Work-related documents
- `mapping-rule-templates.json` - Copy-paste templates

### Loading Multiple Files
Currently, load one file at a time:
```bash
dotnet run -- simulate --scan-id abc --rules examples/mapping-rules.json
```

Future versions may support loading multiple rule files or directories.

---

## Support

### Questions?
- Review examples in `examples/` directory
- Check `docs/MANUAL-TEST-PLAN.md` for test scenarios
- See `DAY3-COMPLETE.md` for CLI usage examples

### Debugging
Use verbose mode to see detailed error messages:
```bash
dotnet run -- simulate --scan-id abc --rules rules.json --verbose
```

---

*Last Updated: 2025-01-03*
*Version: 1.0*
*Part of: DocsUnmessed Phase 2 - Rules Engine*
