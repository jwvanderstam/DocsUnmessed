# DocsUnmessed CLI Reference

## Overview

The DocsUnmessed CLI provides commands for assessing, planning, and executing file organization operations.

## Global Options

```bash
--help, -h      Show help information
--version, -v   Show version information
```

## Commands

### assess

Scan and assess your current storage setup across multiple providers.

**Usage:**
```bash
docsunmessed assess [options]
```

**Options:**
- `--providers <list>` - Comma-separated list of providers to scan
  - Available: `fs_local`, `onedrive_local`, `onedrive_api`, `gdrive_local`, `gdrive_api`, `dropbox_local`, `dropbox_api`
  - Default: `fs_local`
- `--root <path>` - Root directory to scan (for fs_local)
  - Default: Current directory
- `--out <file>` - Output file for results (JSON)
  - Optional

**Examples:**

Scan local Documents folder:
```bash
docsunmessed assess --providers fs_local --root "C:\Users\Me\Documents" --out report.json
```

Scan multiple providers:
```bash
docsunmessed assess --providers fs_local,onedrive_local --out assessment.json
```

Scan current directory:
```bash
docsunmessed assess
```

**Output:**

Console output shows:
- Scan ID
- Progress (items processed)
- Summary statistics (files, folders, size, depth, issues)

JSON output includes:
- Full scan metadata
- Item inventory with hashes
- Statistics by provider
- Duplicate detection results
- Validation issues

---

### simulate

Create a what-if migration plan without making any changes.

**Usage:**
```bash
docsunmessed simulate [options]
```

**Options:**
- `--scan-id <id>` - ID of a previous assessment scan (required)
- `--batch-size <number>` - Number of operations per batch
  - Default: 500
- `--out <file>` - Output file for migration plan (JSON)
  - Optional

**Examples:**

Create migration plan from scan:
```bash
docsunmessed simulate --scan-id abc123def456 --out plan.json
```

With custom batch size:
```bash
docsunmessed simulate --scan-id abc123 --batch-size 1000 --out plan.json
```

**Output:**

Console output shows:
- Plan ID
- Total operations
- Operations by type (copy, move, rename, etc.)
- Metrics (depth reduction, duplicates eliminated, compliance uplift)

JSON output includes:
- Full migration plan
- Operation details (source, target, type)
- Metrics and impact analysis
- Dependency ordering

---

### migrate

**(Coming Soon)** Execute a migration plan.

**Usage:**
```bash
docsunmessed migrate [options]
```

**Options:**
- `--plan <file>` - Migration plan file (JSON, required)
- `--batch-size <number>` - Operations per batch
  - Default: 500
- `--off-hours <range>` - Time window for execution (HH:mm-HH:mm)
  - Optional, e.g., "22:00-06:00"
- `--non-destructive` - Use copy instead of move
  - Default: true
- `--verify` - Verify hashes after operations
  - Default: true
- `--max-retries <number>` - Maximum retry attempts
  - Default: 3

**Examples:**

Execute migration plan:
```bash
docsunmessed migrate --plan migration-plan.json
```

Execute during off-hours only:
```bash
docsunmessed migrate --plan plan.json --off-hours "22:00-06:00"
```

Execute with custom settings:
```bash
docsunmessed migrate --plan plan.json --batch-size 1000 --max-retries 5
```

---

### validate

**(Coming Soon)** Validate folder structure and naming conventions.

**Usage:**
```bash
docsunmessed validate [options]
```

**Options:**
- `--path <directory>` - Path to validate (required)
- `--naming <file>` - Naming template file (JSON)
- `--policy <file>` - TIA blueprint file (YAML)
- `--fix` - Auto-fix naming violations
  - Default: false (report only)

**Examples:**

Validate folder structure:
```bash
docsunmessed validate --path "C:\OneDrive\02_Work\Projects"
```

Validate with naming template:
```bash
docsunmessed validate --path "./Documents" --naming naming-template.json
```

Validate and auto-fix:
```bash
docsunmessed validate --path "./Photos" --naming template.json --fix
```

---

## Exit Codes

- `0` - Success
- `1` - General error
- `2` - Invalid arguments
- `3` - File not found
- `4` - Permission denied

## Configuration Files

### TIA Blueprint (YAML)

Place in: `./config/tia-blueprint.yaml`

```yaml
version: 1
root:
  - key: personal
    label: "01_Personal"
rules:
  maxDepth: 4
  numericPrefixes: true
primary:
  oneDriveRoot: "OneDrive://user@example.com/"
```

### Naming Template (JSON)

Place in: `./config/naming-template.json`

```json
{
  "name": "StandardDateContextTitle",
  "pattern": "{YYYY}{MM}{DD}_{Context}_{Title}_v{NN}.{ext}",
  "requirements": {
    "tokens": ["YYYY", "MM", "DD", "Context", "Title", "NN", "ext"]
  }
}
```

### Mapping Rules (JSON)

Place in: `./config/rules/`

```json
{
  "name": "Downloads-PDF-Archive",
  "match": {
    "pathRegex": "(?i)(Downloads/).*\\.pdf$",
    "ageDaysMin": 90
  },
  "target": {
    "location": "OneDrive://user/03_Tech/99_Archive/"
  }
}
```

## Environment Variables

- `DOCSUNMESSED_CONFIG_DIR` - Override default config directory
- `DOCSUNMESSED_LOG_LEVEL` - Set log level (Debug, Info, Warning, Error)
- `DOCSUNMESSED_NO_COLOR` - Disable colored output

## Common Workflows

### 1. Initial Assessment

```bash
# Scan your documents
docsunmessed assess --providers fs_local --root "C:\Users\Me\Documents" --out assessment.json

# Review the assessment.json file
# Check statistics, duplicates, and issues
```

### 2. Plan Migration

```bash
# Create a migration plan
docsunmessed simulate --scan-id <scan-id-from-assessment> --out plan.json

# Review plan.json
# Check proposed operations and metrics
```

### 3. Execute Migration (Coming Soon)

```bash
# Execute the plan
docsunmessed migrate --plan plan.json

# Monitor progress in console
# Check audit logs for details
```

### 4. Validate Results (Coming Soon)

```bash
# Validate the new structure
docsunmessed validate --path "C:\OneDrive\02_Work" --naming naming-template.json

# Fix any issues
docsunmessed validate --path "C:\OneDrive\02_Work" --naming naming-template.json --fix
```

## Troubleshooting

### Permission Errors

Run with elevated privileges if scanning system folders:
```bash
# Windows (PowerShell as Administrator)
dotnet run -- assess --root "C:\Program Files"

# macOS/Linux (sudo)
sudo dotnet run -- assess --root "/usr/local"
```

### Large File Sets

For very large scans (>100k files), increase batch size:
```bash
docsunmessed assess --providers fs_local --root "/large/directory"
# Let it complete, note the scan ID

docsunmessed simulate --scan-id <scan-id> --batch-size 2000
```

### Network Shares

For UNC paths on Windows:
```bash
docsunmessed assess --providers fs_local --root "\\NAS\Share\Documents" --out nas-scan.json
```

## Support

For issues and questions:
- GitHub Issues: https://github.com/yourusername/DocsUnmessed/issues
- Documentation: https://github.com/yourusername/DocsUnmessed/docs
