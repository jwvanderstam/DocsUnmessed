# DocsUnmessed Rules Engine - Quick Start Guide

## Welcome! ??

This guide will get you organizing files with the Rules Engine in 5 minutes.

---

## Prerequisites

- ? .NET 10 SDK installed
- ? Git (to clone the repository)
- ? Text editor (VS Code, Visual Studio, or any editor)
- ? Windows, macOS, or Linux

---

## Step 1: Get DocsUnmessed

```bash
# Clone the repository
git clone https://github.com/yourusername/DocsUnmessed.git
cd DocsUnmessed

# Verify it builds
dotnet build DocsUnmessed.csproj
```

---

## Step 2: Create Test Data (Optional)

If you want to test with sample files:

```bash
# Run the test data generator
pwsh scripts/create-test-data.ps1

# This creates:
# test-data/
# ??? Downloads/ (mixed files)
# ??? Documents/ (office files)
# ??? Pictures/ (images)
# ??? Videos/ (video files)
```

---

## Step 3: Assess Your Files

Scan a directory to see what you have:

```bash
dotnet run -- assess --providers fs_local --root ./test-data --out my-scan.json
```

**What This Does**:
- Scans all files in `test-data/`
- Creates inventory with metadata
- Saves to `my-scan.json`
- Shows statistics

**Output Example**:
```
DocsUnmessed - File Assessment
==============================

Scanning provider: fs_local
Root: ./test-data

Found 15 files
Total size: 125 KB

Scan ID: a1b2c3d4
Results saved to: my-scan.json
```

---

## Step 4: Choose a Rule Configuration

Pick an example configuration that fits your needs:

**General Purpose**:
```bash
examples/mapping-rules.json
```

**Media Files**:
```bash
examples/mapping-rule-media.json
```

**Developer Files**:
```bash
examples/mapping-rule-development.json
```

**Work Documents**:
```bash
examples/mapping-rule-work.json
```

**Archive Old Files**:
```bash
examples/mapping-rule-archive.json
```

Or create your own! See `examples/mapping-rule-templates.json` for templates.

---

## Step 5: Validate Your Rules

Before using rules, validate them:

```bash
dotnet run -- validate --rules examples/mapping-rules.json
```

**Expected Output**:
```
DocsUnmessed - Rule Validation
================================

Validating rules from: examples/mapping-rules.json

? Rule validation passed with no issues
```

If you see errors, fix them before continuing.

---

## Step 6: Simulate Organization

See where files would go WITHOUT actually moving them:

```bash
dotnet run -- simulate --scan-id a1b2c3d4 --rules examples/mapping-rules.json --out suggestions.json
```

**Replace `a1b2c3d4` with your actual scan ID from Step 3.**

**Output Shows**:
- How many files got suggestions
- Coverage percentage
- Grouping by target location
- Grouping by rule name
- Sample suggestions (first 10)

**Example Output**:
```
Rules Evaluation Results:
  Files with suggestions: 12
  Files without suggestions: 3
  Coverage: 80.0%

Suggestions by Target Location:
  OneDrive://04_Media/Photos/: 5 files
  OneDrive://03_Tech/Archive/: 4 files
  OneDrive://02_Work/Current/: 3 files

Sample Suggestions (first 10):
  File: photo.jpg
  Current: ./test-data/Pictures/photo.jpg
  Target: OneDrive://04_Media/Photos/photo.jpg
  Rule: Photos-Organization
  Confidence: 90%
  Reason: Matched extension: .jpg
```

---

## Step 7: Review Suggestions

Open the suggestions file to review:

```bash
# View summary
cat suggestions.json | jq '.summary'

# View all suggestions
cat suggestions.json | jq '.suggestions[] | {name: .sourceName, target: .targetPath}'

# Count by target
cat suggestions.json | jq '[.suggestions[] | .targetPath] | group_by(.) | map({path: .[0], count: length})'
```

---

## Step 8: Execute Migration (Coming Soon)

When ready to actually move files:

```bash
# This will be available soon
dotnet run -- migrate --plan suggestions.json --batch-size 100
```

**Current Status**: Migration execution is planned for Week 2.

---

## Common Workflows

### Workflow 1: Test on Small Set

```bash
# 1. Create test data
pwsh scripts/create-test-data.ps1

# 2. Scan test data
dotnet run -- assess --providers fs_local --root ./test-data --out test-scan.json

# 3. Validate rules
dotnet run -- validate --rules examples/mapping-rules.json

# 4. Simulate
dotnet run -- simulate --scan-id <scan-id> --rules examples/mapping-rules.json --out test-suggestions.json

# 5. Review
cat test-suggestions.json | jq '.summary'
```

---

### Workflow 2: Organize Photos

```bash
# 1. Scan photo directory
dotnet run -- assess --providers fs_local --root C:\Users\Me\Pictures --out photo-scan.json

# 2. Validate media rules
dotnet run -- validate --rules examples/mapping-rule-media.json

# 3. Simulate
dotnet run -- simulate --scan-id <scan-id> --rules examples/mapping-rule-media.json --out photo-org.json

# 4. Review
cat photo-org.json | jq '.summary'
```

---

### Workflow 3: Clean Up Downloads

```bash
# 1. Scan downloads
dotnet run -- assess --providers fs_local --root C:\Users\Me\Downloads --out downloads-scan.json

# 2. Validate archive rules
dotnet run -- validate --rules examples/mapping-rule-archive.json

# 3. Simulate
dotnet run -- simulate --scan-id <scan-id> --rules examples/mapping-rule-archive.json --out cleanup.json

# 4. Review
cat cleanup.json | jq '.summary'
```

---

### Workflow 4: Developer Workspace

```bash
# 1. Scan project folder
dotnet run -- assess --providers fs_local --root C:\Projects --out dev-scan.json

# 2. Validate development rules
dotnet run -- validate --rules examples/mapping-rule-development.json

# 3. Simulate
dotnet run -- simulate --scan-id <scan-id> --rules examples/mapping-rule-development.json --out dev-org.json

# 4. Review
cat dev-org.json | jq '.summary'
```

---

## Creating Custom Rules

### Method 1: Copy a Template

```bash
# Copy template file
cp examples/mapping-rule-templates.json my-rules.json

# Edit in your favorite editor
code my-rules.json  # or nano, vim, etc.

# Validate
dotnet run -- validate --rules my-rules.json

# Use
dotnet run -- simulate --scan-id <id> --rules my-rules.json --out my-suggestions.json
```

---

### Method 2: Start from Example

```bash
# Copy an example
cp examples/mapping-rule-media.json my-custom-rules.json

# Modify to your needs
code my-custom-rules.json

# Validate
dotnet run -- validate --rules my-custom-rules.json

# Use
dotnet run -- simulate --scan-id <id> --rules my-custom-rules.json --out suggestions.json
```

---

### Method 3: Create from Scratch

```json
[
  {
    "name": "My-First-Rule",
    "match": {
      "extensions": ["pdf", "docx"]
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

Save as `my-first-rules.json`, then:

```bash
dotnet run -- validate --rules my-first-rules.json
dotnet run -- simulate --scan-id <id> --rules my-first-rules.json --out results.json
```

---

## Troubleshooting

### Problem: Scan ID not found

**Solution**: Make sure you're using the correct scan ID from the assess command output.

---

### Problem: Rules validation fails

**Solution**: 
1. Check the error messages
2. Fix the issues in your rules file
3. Re-validate
4. See `docs/RULE-CONFIGURATION-FORMAT.md` for details

---

### Problem: No files match any rules

**Solution**:
1. Check your match criteria
2. Verify file paths match your regex
3. Try broader extensions
4. Lower priority rules might be more general

---

### Problem: Too many matches

**Solution**:
1. Use more specific rules
2. Add age constraints
3. Use path regex to target specific folders
4. Increase priority for specific rules

---

## Next Steps

### Learn More
- Read `docs/RULE-CONFIGURATION-FORMAT.md` for complete rule reference
- Check `docs/FEATURE-SHOWCASE.md` for advanced features
- Review `examples/` directory for more rule examples

### Customize
- Create your own rule configurations
- Test on sample data before real files
- Build a library of rules for different scenarios

### Get Help
- Check documentation in `docs/` directory
- Review issue tracker
- Ask questions in discussions

---

## Command Reference

### Assess
```bash
dotnet run -- assess --providers fs_local --root <path> --out <file>
```

### Validate
```bash
dotnet run -- validate --rules <rules-file> [--verbose]
```

### Simulate
```bash
dotnet run -- simulate --scan-id <id> --rules <rules-file> --out <file> [--verbose]
```

### Help
```bash
dotnet run -- help
```

---

## Tips & Tricks

### Tip 1: Test on Copies
Always test on copies of your files first, not originals.

### Tip 2: Use Verbose Mode
Add `--verbose` to any command to see detailed error information.

### Tip 3: Start Small
Begin with a small set of files and a few simple rules.

### Tip 4: Validate Everything
Always validate rules before using them.

### Tip 5: Review Before Executing
Always review suggestions carefully before executing migrations.

### Tip 6: Use Version Control
Keep your rule configurations in version control (git).

### Tip 7: Document Your Rules
Add comments in JSON (when supported) or keep a separate notes file.

---

## Success Checklist

- [ ] DocsUnmessed cloned and building
- [ ] Test data created (optional)
- [ ] First assessment completed
- [ ] Rules validated successfully
- [ ] Simulation completed
- [ ] Suggestions reviewed
- [ ] Ready for next steps!

---

## Congratulations! ??

You're now ready to use DocsUnmessed to organize your files!

**What's Next**:
- Experiment with different rule configurations
- Organize real directories
- Build your own rule library
- Share your rules with others

---

*DocsUnmessed Quick Start Guide*  
*Version 1.0*  
*Date: January 3, 2025*
