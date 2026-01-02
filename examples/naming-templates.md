# Example Naming Templates

This file contains ready-to-use naming templates for various scenarios.

---

## Basic Templates

### Simple Date-Based
```
{Year}/{Month}/{Name}.{Extension}
```
**Output**: `2025/01/Document.pdf`

### Provider-Based
```
{Provider}/{Category}/{Name}.{Extension}
```
**Output**: `OneDrive/Work/Report.docx`

### Flat with Date Prefix
```
{Date:yyyy-MM-dd}_{Name}.{Extension}
```
**Output**: `2025-01-15_Document.pdf`

---

## Archive Templates

### Monthly Archive
```
Archive/{Year}/{Month}/{Name|sanitize}.{Extension}
```
**Output**: `Archive/2025/01/Document.pdf`

### Dated Archive
```
Archive/{Date:yyyy-MM}/{Category}/{Name}.{Extension}
```
**Output**: `Archive/2025-01/Work/Report.docx`

### Timestamped Backup
```
Backup/{Date:yyyy-MM-dd}/{DateTime:HHmmss}_{Name}.{Extension}
```
**Output**: `Backup/2025-01-15/143000_Database.bak`

---

## Photo Organization

### Year/Month Structure
```
Photos/{Year}/{Month}/{Date:yyyy-MM-dd}_{Counter:000}.{Extension}
```
**Output**: `Photos/2025/01/2025-01-15_042.jpg`

### Event-Based
```
Photos/{Year}/{Category}/{Date:yyyy-MM-dd}_{Name}.{Extension}
```
**Output**: `Photos/2025/Vacation/2025-01-15_Beach.jpg`

### Timestamp Organized
```
Photos/{Year}/{Date:MMM}/{Date:dd}_{Time:HHmmss}.{Extension}
```
**Output**: `Photos/2025/Jan/15_143000.jpg`

---

## Document Templates

### Project Structure
```
Projects/{Category}/{Year}/{Date:yyyy-MM-dd}_{Name|sanitize}.{Extension}
```
**Output**: `Projects/Work/2025/2025-01-15_Proposal.docx`

### Client/Project
```
{Category}/{Provider}/{Year}-{Month}/{Name|title}.{Extension}
```
**Output**: `ClientA/OneDrive/2025-01/Project Proposal.docx`

### Numbered Documents
```
Documents/{Year}/{Counter:0000}_{Date:yyyy-MM-dd}_{Name|lower}.{Extension}
```
**Output**: `Documents/2025/0042_2025-01-15_report.pdf`

---

## Cloud Storage Templates

### OneDrive Organization
```
OneDrive/{Type}/{Category}/{Year}/{Name|sanitize}.{Extension}
```
**Output**: `OneDrive/Documents/Work/2025/Report.docx`

### Multi-Provider
```
{Provider}/Organized/{Date:yyyy/MM}/{Category}/{Name}.{Extension}
```
**Output**: `GoogleDrive/Organized/2025/01/Work/Document.pdf`

### Clean Structure
```
{Provider}/{Year}/{Date:MMM}/{Name|sanitize|lower}.{Extension}
```
**Output**: `Dropbox/2025/Jan/my_document.pdf`

---

## Development Templates

### Code Repository
```
Code/{Category}/{Date:yyyy-MM-dd}_{Name|alphanumeric|lower}.{Extension}
```
**Output**: `Code/Python/2025-01-15_myproject.py`

### Backup with Version
```
Backup/{Year}/{Month}/{Name}_{Date:yyyyMMdd}_{Counter:000}.{Extension}
```
**Output**: `Backup/2025/01/Database_20250115_001.bak`

### Log Files
```
Logs/{Year}/{Month}/{Date:yyyy-MM-dd}_{Type|lower}.{Extension}
```
**Output**: `Logs/2025/01/2025-01-15_error.log`

---

## Media Templates

### Music Library
```
Music/{Category}/{Name|title}.{Extension}
```
**Output**: `Music/Rock/My Favorite Song.mp3`

### Video Archive
```
Videos/{Year}/{Category}/{Date:yyyy-MM-dd}_{Name|sanitize}.{Extension}
```
**Output**: `Videos/2025/Family/2025-01-15_Birthday.mp4`

### Audio Recordings
```
Recordings/{Date:yyyy-MM-dd}/{Time:HHmmss}_{Counter:000}.{Extension}
```
**Output**: `Recordings/2025-01-15/143000_001.m4a`

---

## Business Templates

### Invoices
```
Invoices/{Year}/{Month}/{Date:yyyy-MM-dd}_{Counter:0000}.{Extension}
```
**Output**: `Invoices/2025/01/2025-01-15_0042.pdf`

### Contracts
```
Contracts/{Category}/{Year}/{Name|upper|alphanumeric}.{Extension}
```
**Output**: `Contracts/Clients/2025/CONTRACT_ACME.pdf`

### Reports
```
Reports/{Year}/{Date:MMM}/{Type}_{Date:yyyy-MM-dd}.{Extension}
```
**Output**: `Reports/2025/Jan/Monthly_2025-01-15.xlsx`

---

## Educational Templates

### Courses
```
Education/{Year}/{Category}/{Name|title}.{Extension}
```
**Output**: `Education/2025/Math/Introduction to Calculus.pdf`

### Assignments
```
Assignments/{Category}/{Year}/{Counter:00}_{Name}.{Extension}
```
**Output**: `Assignments/ComputerScience/2025/01_FirstProject.zip`

### Notes
```
Notes/{Year}/{Date:MMM}/{Date:dd}_{Category}_{Name|lower}.{Extension}
```
**Output**: `Notes/2025/Jan/15_math_calculus.md`

---

## Personal Templates

### Journal
```
Journal/{Year}/{Month}/{Date:yyyy-MM-dd}_{Name|title}.{Extension}
```
**Output**: `Journal/2025/01/2025-01-15_Daily Entry.md`

### Recipes
```
Recipes/{Category}/{Name|title|sanitize}.{Extension}
```
**Output**: `Recipes/Desserts/Chocolate Cake.pdf`

### Finance
```
Finance/{Year}/{Category}/{Date:yyyy-MM-dd}_{Name}.{Extension}
```
**Output**: `Finance/2025/Statements/2025-01-15_Bank.pdf`

---

## Advanced Templates

### Multi-Function
```
{Provider}/{Year}/{Date:MMM}/{Name|sanitize|lower|truncate(50)}.{Extension|lower}
```
**Output**: `onedrive/2025/jan/my_very_long_document_name_that_gets_tru....pdf`

### Conditional Structure
```
{Type}/{Year}/{Month}/{Date:dd}_{Name|sanitize}.{Extension}
```
**Output**: `Documents/2025/01/15_Report.pdf` or `Media/2025/01/15_Video.mp4`

### Full Path Generation
```
{Provider}/Organized/{Date:yyyy}/{Date:MM}-{Date:MMM}/{Type}/{Extension}/{Name|sanitize|lower}.{Extension}
```
**Output**: `OneDrive/Organized/2025/01-Jan/Documents/pdf/report.pdf`

---

## Specialized Use Cases

### Medical Records
```
Medical/{Year}/{Category}/{Date:yyyy-MM-dd}_{Type|upper}.{Extension}
```
**Output**: `Medical/2025/Lab/2025-01-15_BLOOD_TEST.pdf`

### Legal Documents
```
Legal/{Category}/{Year}/{Date:yyyy-MM-dd}_{Counter:0000}_{Name|upper}.{Extension}
```
**Output**: `Legal/Contracts/2025/2025-01-15_0042_AGREEMENT.pdf`

### Research Papers
```
Research/{Year}/{Category}/{Name|title}_{Date:yyyyMMdd}.{Extension}
```
**Output**: `Research/2025/Physics/Quantum Theory_20250115.pdf`

---

## Migration Templates

### Consolidation
```
Consolidated/{Provider}/{Type}/{Year}/{Name|sanitize}.{Extension}
```
**Output**: `Consolidated/OneDrive/Documents/2025/Report.pdf`

### Cleanup
```
Organized/{Date:yyyy-MM}/{Type}/{Name|sanitize|lower}.{Extension}
```
**Output**: `Organized/2025-01/Documents/report.pdf`

### Deduplication
```
Unique/{Extension}/{Name}_{Counter:000}.{Extension}
```
**Output**: `Unique/pdf/Document_001.pdf`

---

## Tips for Creating Templates

### 1. Start Simple
Begin with basic templates and add complexity as needed:
```
{Year}/{Name}.{Extension}  ? Start here
{Year}/{Month}/{Name}.{Extension}  ? Add detail
{Year}/{Month}/{Category}/{Name|sanitize}.{Extension}  ? Add more
```

### 2. Test with Sample Data
Always test templates before applying to real files:
```csharp
var context = new TemplateContext
{
    FileName = "Test Document",
    Date = new DateTime(2025, 1, 15)
};
var result = engine.Process(template, context);
Console.WriteLine(result);
```

### 3. Consider Path Length
Windows has 260 character path limit:
```
// Good - short and clean
{Year}/{Month}/{Name}.{Extension}

// Risky - might exceed limit
{Provider}/{Category}/{Subcategory}/{Year}/{Month}/{Day}/{LongName...}
```

### 4. Use Functions Wisely
Don't over-use functions:
```
// Good
{Name|sanitize}.{Extension}

// Over-complicated
{Name|sanitize|lower|trim|truncate(50)|alphanumeric}.{Extension}
```

---

*Last Updated: January 3, 2025*  
*Total Examples: 50+*
