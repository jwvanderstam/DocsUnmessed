# DocsUnmessed - Migration Guide

## Complete File Migration Workflow

This guide covers the complete workflow for migrating and organizing your files using DocsUnmessed's smart categorization and migration features.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Migration Workflow](#migration-workflow)
3. [Command Reference](#command-reference)
4. [Smart Categorization](#smart-categorization)
5. [Exclusion Features](#exclusion-features)
6. [Conflict Resolution](#conflict-resolution)
7. [Examples & Scenarios](#examples--scenarios)
8. [Troubleshooting](#troubleshooting)
9. [Best Practices](#best-practices)

---

## Quick Start

### 5-Minute Migration

```sh
# 1. Scan your files (creates scan ID)
dotnet run -- assess --root "C:\Users\Me\Documents"

# 2. Preview migration (dry-run)
dotnet run -- migrate --scan-id <scan-id> --dry-run

# 3. Execute migration
dotnet run -- migrate --scan-id <scan-id>
```

**Result**: Files organized in `migrated/` directory by category!

---

## Migration Workflow

### Step 1: Assessment

Scan your files to create an inventory:

```sh
dotnet run -- assess --root "C:\Users\Me\Documents" --out scan.json
```

**Output**:
```
? Database initialized
DocsUnmessed - File Assessment
==============================

?? Scanning root: C:\Users\Me\Documents
?? Providers: fs_local

Scan ID: abc123xyz456

?? Scanning provider: fs_local
? Scan complete: 1,234 items found (987 files, 247 folders) in 2.3s
?? Saving 1,234 items (5.2 GB) to database...
? Items saved successfully in 0.5s

? Finalizing scan and computing statistics...

?? Scan Summary:
  Total Files: 987
  Total Folders: 247
  Total Size: 5.2 GB
  Max Depth: 5
  Files with Issues: 3

?? Results exported to: scan.json

? Assessment complete!
```

**Save the Scan ID** - You'll need it for migration!

### Step 2: Preview (Dry-Run)

Preview what will happen without moving files:

```sh
dotnet run -- migrate --scan-id abc123xyz456 --dry-run
```

**Output**:
```
DocsUnmessed - File Migration
==============================

?? Scan Information:
  Scan ID: abc123xyz456
  Total Files: 987
  Total Size: 5.2 GB
  Scan Date: 2025-01-15 10:30:00

?? Files to migrate: 950 (after exclusions)

?? Categorizing files...

?? File Categories:
  Document    :    345 files (  1.2 GB)
  Picture     :    412 files (  3.1 GB)
  Music       :     87 files (450.0 MB)
  Video       :     45 files (350.0 MB)
  Archive     :     23 files (120.0 MB)
  Code        :     18 files ( 15.0 MB)
  Data        :     12 files (  8.0 MB)
  Other       :      8 files (  2.0 MB)

?? DRY RUN MODE - No files will be moved

?? Preview (showing up to 20 files):

[DRY-RUN] C:\Users\Me\Documents\photo1.jpg
       ? migrated\Pictures\photo1.jpg

[DRY-RUN] C:\Users\Me\Documents\report.pdf
       ? migrated\Documents\report.pdf

[DRY-RUN] C:\Users\Me\Documents\song.mp3
       ? migrated\Music\song.mp3

... (17 more previews)

? Dry run complete. 950 files would be migrated.
   Run without --dry-run to execute the migration.
```

### Step 3: Execute Migration

Run the actual migration:

```sh
dotnet run -- migrate --scan-id abc123xyz456
```

**Output**:
```
DocsUnmessed - File Migration
==============================

?? Scan Information:
  Scan ID: abc123xyz456
  Total Files: 987
  Total Size: 5.2 GB
  Scan Date: 2025-01-15 10:30:00

?? Files to migrate: 950 (after exclusions)

?? Categorizing files...

?? File Categories:
  Document    :    345 files (  1.2 GB)
  Picture     :    412 files (  3.1 GB)
  ... (more categories)

??  About to migrate 950 files.
Do you want to continue? (y/N): y

?? Starting migration...

?? Progress: 950/950 (100.0%) | 125 files/sec | 950 succeeded, 0 failed

?? Migration Results:
  Total files: 950
  Succeeded: 950
  Failed: 0
  Total size moved: 5.1 GB
  Duration: 7.6 seconds
  Average speed: 125 files/sec

? Migration complete!
```

---

## Command Reference

### assess Command

Scan files and create inventory.

**Syntax**:
```sh
dotnet run -- assess [options]
```

**Options**:
| Option | Description | Default |
|--------|-------------|---------|
| `--root <path>` | Root directory to scan | Documents folder |
| `--providers <list>` | Comma-separated providers | fs_local |
| `--out <path>` | Output JSON file | None |
| `--compute-hash` | Enable hash computation | false |
| `--exclude-default` | Exclude system directories | true |
| `--exclude-dirs <list>` | Custom exclusions | None |

**Examples**:
```sh
# Basic scan
dotnet run -- assess

# Scan specific directory
dotnet run -- assess --root "C:\Data"

# Scan with hash computation (slower but enables duplicate detection)
dotnet run -- assess --compute-hash

# Exclude system directories
dotnet run -- assess --exclude-default
```

### migrate Command

Execute file migration with categorization.

**Syntax**:
```sh
dotnet run -- migrate --scan-id <id> [options]
```

**Options**:
| Option | Description | Default |
|--------|-------------|---------|
| `--scan-id <id>` | **Required** - Scan ID from assess | None |
| `--target <path>` | Target directory | migrated |
| `--categorize` | Enable categorization | true |
| `--no-categorize` | Disable categorization | false |
| `--dry-run` | Preview without moving | false |
| `--force` | Skip confirmation | false |
| `--out <path>` | Save report to JSON | None |
| `--conflict <mode>` | rename/skip/overwrite | rename |
| `--max-preview <n>` | Max preview items | 20 |
| `--exclude-default` | Exclude system dirs | true |
| `--verbose` | Detailed errors | false |

**Examples**:
```sh
# Dry-run (preview)
dotnet run -- migrate --scan-id abc123 --dry-run

# Execute with categories
dotnet run -- migrate --scan-id abc123

# Custom target directory
dotnet run -- migrate --scan-id abc123 --target "organized"

# Force (no prompt)
dotnet run -- migrate --scan-id abc123 --force

# Save report
dotnet run -- migrate --scan-id abc123 --out report.json

# Skip categorization
dotnet run -- migrate --scan-id abc123 --no-categorize
```

---

## Smart Categorization

Files are automatically categorized by extension.

### Supported Categories

#### ?? Pictures (13 extensions)
`.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.svg`, `.webp`, `.ico`, `.tif`, `.tiff`, `.raw`, `.heic`, `.heif`

**Target**: `migrated/Pictures/`

#### ?? Music (9 extensions)
`.mp3`, `.wav`, `.flac`, `.aac`, `.ogg`, `.wma`, `.m4a`, `.opus`, `.aiff`

**Target**: `migrated/Music/`

#### ?? Videos (10 extensions)
`.mp4`, `.avi`, `.mkv`, `.mov`, `.wmv`, `.flv`, `.webm`, `.m4v`, `.mpg`, `.mpeg`

**Target**: `migrated/Videos/`

#### ?? Documents (11 extensions)
`.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.txt`, `.rtf`, `.odt`, `.ods`, `.odp`

**Target**: `migrated/Documents/`

#### ?? Archives (7 extensions)
`.zip`, `.rar`, `.7z`, `.tar`, `.gz`, `.bz2`, `.xz`

**Target**: `migrated/Archives/`

#### ?? Code (15 extensions)
`.cs`, `.java`, `.py`, `.js`, `.ts`, `.cpp`, `.c`, `.h`, `.go`, `.rs`, `.rb`, `.php`, `.html`, `.css`, `.sql`

**Target**: `migrated/Code/`

#### ?? Data (8 extensions)
`.json`, `.xml`, `.csv`, `.yaml`, `.yml`, `.db`, `.sqlite`, `.log`

**Target**: `migrated/Data/`

#### ? Other
All other file types

**Target**: `migrated/Other/`

### Example Organization

**Before**:
```
Documents/
??? photo1.jpg
??? photo2.png
??? report.pdf
??? song.mp3
??? video.mp4
??? archive.zip
```

**After** (with `--categorize`):
```
migrated/
??? Pictures/
?   ??? photo1.jpg
?   ??? photo2.png
??? Documents/
?   ??? report.pdf
??? Music/
?   ??? song.mp3
??? Videos/
?   ??? video.mp4
??? Archives/
    ??? archive.zip
```

---

## Exclusion Features

### Default Exclusions

By default, these directories are excluded:

#### System Directories (30+)
- Windows: `$Recycle.Bin`, `Windows`, `Program Files`, `AppData`
- macOS: `.Spotlight-V100`, `Library`, `System`, `Applications`
- Linux: `proc`, `sys`, `dev`, `boot`
- Common: `.git`, `.svn`, `node_modules`, `.vs`, `bin`, `obj`

#### User Directories (6)
- `Downloads`
- `Desktop`
- `Pictures`
- `Music`
- `Videos`
- `Documents` (can be overridden)

### Custom Exclusions

```sh
# Add custom exclusions
dotnet run -- migrate --scan-id abc123 --exclude-dirs "TempFiles,OldBackups"

# Disable default exclusions
dotnet run -- migrate --scan-id abc123 --no-exclude-default

# Exclude file patterns
dotnet run -- migrate --scan-id abc123 --exclude-patterns "*.tmp,*.bak"
```

---

## Conflict Resolution

When target file already exists, choose a strategy:

### 1. Rename (Default)

Creates unique name by appending number:
```
file.txt ? file (1).txt
file.txt ? file (2).txt
```

**Usage**:
```sh
dotnet run -- migrate --scan-id abc123 --conflict rename
```

### 2. Skip

Skips file if target exists (doesn't move):
```sh
dotnet run -- migrate --scan-id abc123 --conflict skip
```

### 3. Overwrite

Replaces existing file (use with caution!):
```sh
dotnet run -- migrate --scan-id abc123 --conflict overwrite
```

---

## Examples & Scenarios

### Scenario 1: Clean Up Downloads Folder

```sh
# 1. Scan Downloads
dotnet run -- assess --root "C:\Users\Me\Downloads"

# 2. Preview (note scan ID from step 1)
dotnet run -- migrate --scan-id <scan-id> --dry-run

# 3. Migrate with categories
dotnet run -- migrate --scan-id <scan-id> --target "Downloads-Organized"
```

**Result**: All downloads organized by type!

### Scenario 2: Migrate Without System Files

```sh
# Scan with exclusions
dotnet run -- assess --root "C:\Data" --exclude-default

# Migrate (already excluded during scan)
dotnet run -- migrate --scan-id <scan-id>
```

### Scenario 3: Large Migration with Report

```sh
# 1. Scan with hash computation
dotnet run -- assess --root "C:\Archive" --compute-hash --out full-scan.json

# 2. Dry-run first
dotnet run -- migrate --scan-id <scan-id> --dry-run --max-preview 50

# 3. Execute with report
dotnet run -- migrate --scan-id <scan-id> --out migration-report.json --force
```

### Scenario 4: Move Only Documents

```sh
# Scan
dotnet run -- assess --root "C:\Mixed"

# Migrate without categorization to flat directory
dotnet run -- migrate --scan-id <scan-id> --no-categorize --target "Documents-Only"
```

---

## Troubleshooting

### Issue: "Scan ID not found"

**Problem**: Scan ID doesn't exist in database.

**Solution**:
1. Check scan ID (copy exact ID from assess output)
2. Ensure database file exists: `docsunmessed.db`
3. Re-run assess if needed

### Issue: "Permission denied"

**Problem**: Cannot access files/folders.

**Solution**:
1. Run as administrator (Windows)
2. Check file permissions
3. Use `--exclude-dirs` to skip problematic directories

### Issue: Migration hangs

**Problem**: Large files or many files.

**Solution**:
1. Migration has 30-second timeout per file
2. Check disk space
3. Close other applications
4. Try smaller batches with exclusions

### Issue: "File already exists"

**Problem**: Target file exists.

**Solution**:
1. Use `--conflict rename` (default)
2. Or `--conflict skip` to avoid
3. Or `--conflict overwrite` (caution!)

---

## Best Practices

### 1. Always Dry-Run First

```sh
# Always preview before executing
dotnet run -- migrate --scan-id <id> --dry-run
```

? **Why**: See what will happen  
? **Benefit**: Catch issues early  
? **Safe**: No actual file moves  

### 2. Save Reports

```sh
# Keep records of migrations
dotnet run -- migrate --scan-id <id> --out report-2025-01-15.json
```

? **Why**: Audit trail  
? **Benefit**: Review later  
? **Useful**: Troubleshooting  

### 3. Use Exclusions Wisely

```sh
# Exclude system directories for faster scans
dotnet run -- assess --exclude-default
```

? **Why**: Faster scans  
? **Benefit**: Avoid locked files  
? **Cleaner**: Only user files  

### 4. Backup First

```sh
# Before large migrations, backup!
```

? **Why**: Safety  
? **Benefit**: Can restore  
? **Peace of mind**: Risk-free  

### 5. Test on Small Set

```sh
# Test with small directory first
dotnet run -- assess --root "C:\Small-Test"
dotnet run -- migrate --scan-id <id>
```

? **Why**: Verify workflow  
? **Benefit**: Build confidence  
? **Learn**: Understand tool  

---

## Performance Tips

### Fast Scanning

```sh
# Skip hash computation for speed
dotnet run -- assess  # Default: no hashing
```

**Speed**: 9,000+ files/sec  
**Trade-off**: No duplicate detection  

### Slow but Complete

```sh
# Enable hashing for duplicates
dotnet run -- assess --compute-hash
```

**Speed**: Depends on file sizes  
**Benefit**: Duplicate detection enabled  

### Optimize Target

```sh
# Use fast SSD for target
dotnet run -- migrate --scan-id <id> --target "D:\Fast-SSD"
```

---

## Next Steps

1. ? **Try It**: Run your first migration
2. ?? **Learn More**: Check [CLI Reference](CLI-REFERENCE.md)
3. ?? **Advanced**: Explore [Rules Engine](RULE-CONFIGURATION-FORMAT.md)
4. ?? **Duplicates**: Enable hash computation for duplicate detection

---

*Migration Guide - January 2025*  
*Version: 1.0*  
*DocsUnmessed - File Organization Made Easy*

