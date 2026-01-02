# Manual Test Data for Rules Engine

## Test Directory Structure

```
test-data/
??? Downloads/
?   ??? old-document.pdf (120 days old)
?   ??? recent-invoice.pdf (10 days old)
?   ??? photo-vacation.jpg (90 days old)
?   ??? temp-file.txt (1 day old)
??? Documents/
?   ??? report-2024.docx (50 days old)
?   ??? spreadsheet.xlsx (30 days old)
?   ??? notes.txt (5 days old)
?   ??? archive/
?       ??? old-report.pdf (200 days old)
??? Pictures/
?   ??? IMG_001.jpg (60 days old)
?   ??? vacation_2024.png (45 days old)
?   ??? screenshot.png (2 days old)
??? Videos/
    ??? clip.mp4 (90 days old)
    ??? recording.mov (15 days old)
```

## Test Rules Configuration

The `examples/mapping-rules.json` file should match these test scenarios.

## Expected Results

### Rule: Downloads-PDF-Archive
- **Matches**: `old-document.pdf` (>90 days, PDF in Downloads)
- **Target**: `OneDrive://03_Tech/99_Archive/`
- **Confidence**: High (composite rule: regex + age)

### Rule: Photos-EXIF-Organization
- **Matches**: `photo-vacation.jpg`, `IMG_001.jpg`, `vacation_2024.png`
- **Target**: `OneDrive://04_Media/Photos/`
- **Confidence**: High (extension + path regex)

### Rule: Recent-Documents
- **Matches**: `notes.txt`, `spreadsheet.xlsx`
- **Target**: `OneDrive://02_Work/Recent/`
- **Confidence**: Medium (extension + age < 30 days)

## Manual Test Scenarios

### Scenario 1: Full Assessment with Rules
```bash
# Step 1: Assess test data
dotnet run -- assess --providers fs_local --root ./test-data --out test-scan.json

# Step 2: Simulate with rules
dotnet run -- simulate --scan-id <scan-id> --rules examples/mapping-rules.json --out test-suggestions.json

# Expected: 3-5 suggestions with grouped statistics
```

### Scenario 2: Test Multiple Rules Matching Same File
```bash
# Create a PDF in Downloads that's also recent
# Should match highest priority rule

# Expected: Priority-based rule selection
```

### Scenario 3: Test Files Without Matches
```bash
# Some files in Documents/ should have no matching rules
# Expected: Report shows files without suggestions
```

### Scenario 4: Export and Verify JSON
```bash
# Simulate with output
dotnet run -- simulate --scan-id <scan-id> --rules examples/mapping-rules.json --out suggestions.json

# Verify JSON structure:
# - Summary statistics
# - Individual suggestions with reasons
# - Confidence scores
# - Conflict policies
```

## Validation Checklist

- [ ] CLI accepts --rules parameter
- [ ] Rules load successfully from JSON
- [ ] Files are evaluated against rules
- [ ] Statistics show correct counts
- [ ] Groupings work (by target, by rule)
- [ ] Sample suggestions display correctly
- [ ] JSON export has correct structure
- [ ] Confidence scores are appropriate
- [ ] Reasons are explanatory
- [ ] No matching rules handled gracefully
- [ ] Multiple matching rules prioritized correctly
- [ ] Verbose mode shows errors properly

## Test Data Creation Script

See `scripts/create-test-data.ps1` for automated test data generation.

## Expected Performance

- **Loading rules**: <100ms
- **Evaluating 100 files**: <1 second
- **Export to JSON**: <100ms
- **Total end-to-end**: <5 seconds

## Success Criteria

1. ? All rules load without errors
2. ? At least 60% of files get suggestions
3. ? Statistics match actual file counts
4. ? Export JSON is valid and complete
5. ? No crashes or unhandled exceptions
6. ? Output is readable and informative
