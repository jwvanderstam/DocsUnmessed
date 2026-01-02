# Naming Templates Guide

## Overview

DocsUnmessed provides a powerful and flexible template system for generating file and folder names. Templates use a simple syntax with variables, formatting, and functions to create consistent, organized naming schemes.

---

## Template Syntax

### Basic Syntax

Templates use curly braces `{}` to denote variables:

```
{VariableName}
```

### With Formatting

Add a colon `:` followed by a format string:

```
{Date:yyyy-MM-dd}
{Counter:000}
```

### With Functions

Add a pipe `|` followed by a function name and optional arguments:

```
{Name|upper}
{Name|replace(_,-)}
{Name|truncate(20)}
```

---

## Available Variables

### File Information

| Variable | Description | Example |
|----------|-------------|---------|
| `Name` or `FileName` | Base file name | `Document` |
| `Extension` or `Ext` | File extension | `pdf` |
| `Type` or `FileType` | File type | `File` or `Folder` |

### Date & Time

| Variable | Description | Example |
|----------|-------------|---------|
| `Year` | Four-digit year | `2025` |
| `Month` | Two-digit month | `01` |
| `Day` | Two-digit day | `15` |
| `Date` | ISO date | `2025-01-15` |
| `DateTime` | Date and time | `2025-01-15_14-30-00` |
| `Time` | Time only | `14-30-00` |

### Context

| Variable | Description | Example |
|----------|-------------|---------|
| `Provider` | Storage provider | `OneDrive` |
| `Category` | File category | `Documents` |
| `Counter` | Numeric counter | `1`, `2`, `3` |

### Custom Variables

You can define custom variables in the template context:

```csharp
var context = new TemplateContext
{
    CustomVariables = new Dictionary<string, string>
    {
        ["Project"] = "DocsUnmessed",
        ["Client"] = "ACME Corp"
    }
};
```

---

## Date Formatting

Use standard .NET date format strings:

### Common Formats

| Format | Example | Description |
|--------|---------|-------------|
| `yyyy-MM-dd` | `2025-01-15` | ISO 8601 date |
| `yyyy/MM/dd` | `2025/01/15` | Slash-separated |
| `dd-MM-yyyy` | `15-01-2025` | European format |
| `MMM dd yyyy` | `Jan 15 2025` | Month name |
| `yyyy-MM-dd_HH-mm` | `2025-01-15_14-30` | Date and time |
| `yyyyMMdd` | `20250115` | Compact date |

### Examples

```
{Date:yyyy-MM-dd}           ? 2025-01-15
{Date:yyyy/MM}              ? 2025/01
{Date:MMM yyyy}             ? Jan 2025
{DateTime:yyyy-MM-dd_HHmm}  ? 2025-01-15_1430
```

---

## Numeric Formatting

Use standard .NET numeric format strings:

### Common Formats

| Format | Example | Description |
|--------|---------|-------------|
| `000` | `042` | Zero-padded 3 digits |
| `0000` | `0042` | Zero-padded 4 digits |
| `D4` | `0042` | Decimal 4 digits |
| `000.##` | `042.5` | With decimals |

### Examples

```
{Counter:000}    ? 001, 042, 123
{Counter:0000}   ? 0001, 0042, 0123
{Counter:D5}     ? 00001, 00042, 00123
```

---

## Built-in Functions

### String Manipulation

#### upper
Converts to uppercase

```
{Name|upper}
Input:  my document
Output: MY DOCUMENT
```

#### lower
Converts to lowercase

```
{Name|lower}
Input:  My Document
Output: my document
```

#### title
Converts to title case

```
{Name|title}
Input:  my document file
Output: My Document File
```

#### trim
Removes leading/trailing whitespace

```
{Name|trim}
Input:  " document "
Output: "document"
```

### Text Replacement

#### replace(old,new)
Replaces text

```
{Name|replace(_,-)}
Input:  my_file_name
Output: my-file-name
```

#### remove(text)
Removes text

```
{Name|remove(_)}
Input:  my_file_name
Output: myfilename
```

### Text Extraction

#### substring(start[,length])
Extracts substring

```
{Name|substring(0,5)}
Input:  HelloWorld
Output: Hello

{Name|substring(5)}
Input:  HelloWorld
Output: World
```

### Sanitization

#### sanitize
Removes invalid filename characters

```
{Name|sanitize}
Input:  my:file<name>
Output: my_file_name
```

#### alphanumeric
Keeps only alphanumeric, underscore, hyphen

```
{Name|alphanumeric}
Input:  my@file#name!
Output: myfilename
```

### Formatting

#### pad(width[,char])
Pads with specified character (default: '0')

```
{Counter|pad(5)}
Input:  42
Output: 00042

{Counter|pad(5,_)}
Input:  42
Output: ___42
```

#### truncate(length[,suffix])
Truncates to length with optional suffix

```
{Name|truncate(10)}
Input:  VeryLongFileName
Output: VeryLon...

{Name|truncate(10,~)}
Input:  VeryLongFileName
Output: VeryLongF~
```

---

## Template Examples

### Basic Organization

```
{Year}/{Month}/{Name}.{Extension}
? 2025/01/Document.pdf
```

### Provider-Based

```
{Provider}/{Category}/{Name}.{Extension}
? OneDrive/Work/Report.docx
```

### Date-Based Archive

```
Archive/{Date:yyyy-MM}/{Name|sanitize}.{Extension}
? Archive/2025-01/Monthly_Report.pdf
```

### Numbered Files

```
{Category}/{Year}/{Counter:000}_{Name}.{Extension}
? Photos/2025/042_vacation.jpg
```

### Project Structure

```
{Project}/{Type}/{Date:yyyy-MM-dd}_{Name|lower}.{Extension}
? DocsUnmessed/Documents/2025-01-15_readme.md
```

### Clean Names

```
{Year}/{Month}/{Name|sanitize|lower}.{Extension}
? 2025/01/my_document_file.pdf
```

### Uppercase Convention

```
{Provider}/{Category}/{Name|upper|alphanumeric}.{Extension}
? ONEDRIVE/DOCUMENTS/MYFILE.DOCX
```

### Complex Template

```
{Provider}/{Year}/{Month}/{Category}/{Date:yyyy-MM-dd}_{Counter:000}_{Name|sanitize|lower}.{Extension}
? OneDrive/2025/01/Work/2025-01-15_042_project_proposal.docx
```

---

## Usage Examples

### C# Code

```csharp
using DocsUnmessed.Services.Templates;

// Create engine
var engine = new TemplateEngine();

// Create context
var context = new TemplateContext
{
    FileName = "My Document",
    Extension = "pdf",
    Provider = "OneDrive",
    Category = "Work",
    Date = DateTime.Now,
    Counter = 42
};

// Process template
var template = "{Provider}/{Year}/{Month}/{Category}/{Name|sanitize}.{Extension}";
var result = engine.Process(template, context);
// ? OneDrive/2025/01/Work/My_Document.pdf
```

### Validation

```csharp
var template = "{Year}/{Month}/{Name}";
var validation = engine.Validate(template);

if (validation.IsValid)
{
    Console.WriteLine("Template is valid!");
}
else
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

### Get Variables

```csharp
var template = "{Year}/{Month}/{Name}.{Extension}";
var variables = engine.GetVariables(template);
// ? ["Year", "Month", "Name", "Extension"]
```

---

## Custom Functions

You can register custom functions:

```csharp
var engine = new TemplateEngine();

// Register custom function
engine.Functions.Register("reverse", (input, args) => 
    new string(input.Reverse().ToArray()));

// Use custom function
var template = "{Name|reverse}";
var context = new TemplateContext { FileName = "hello" };
var result = engine.Process(template, context);
// ? "olleh"
```

### Function Signature

```csharp
Func<string, string[], string>
```

- **input**: The resolved variable value
- **args**: Function arguments
- **returns**: Transformed value

---

## Best Practices

### 1. Keep Templates Simple

**Good**:
```
{Year}/{Month}/{Name}.{Extension}
```

**Too Complex**:
```
{Year|pad(4)}_{Month|pad(2)}_{Day|pad(2)}/{Category|upper|truncate(10)}/{Name|sanitize|lower|alphanumeric|truncate(50)}.{Extension|lower}
```

### 2. Use Sanitization

Always sanitize user-provided names:

```
{Name|sanitize}
```

### 3. Consistent Formatting

Use consistent date formats across templates:

```
{Date:yyyy-MM-dd}  ?
{Date:dd/MM/yyyy}  ? (pick one)
```

### 4. Document Your Templates

Add comments explaining template purpose:

```
// Archive structure: Year/Month/filename
{Year}/{Month}/{Name}.{Extension}
```

### 5. Validate Before Use

```csharp
var validation = engine.Validate(template);
if (!validation.IsValid)
{
    // Handle error
}
```

---

## Common Patterns

### Dated Archives

```
Archive/{Year}/{Month}/{Name|sanitize}.{Extension}
Archive/{Date:yyyy-MM}/{Category}/{Name}.{Extension}
Archive/{DateTime:yyyy-MM-dd_HHmm}_{Name}.{Extension}
```

### Project Organization

```
{Project}/{Category}/{Type}/{Name}.{Extension}
Projects/{Project}/{Year}/{Month}/{Name}.{Extension}
{Client}/{Project}/{Date:yyyy-MM-dd}_{Name}.{Extension}
```

### Cloud Storage

```
{Provider}/{Category}/{Year}/{Name}.{Extension}
{Provider}/Organized/{Type}/{Extension}/{Name}.{Extension}
{Provider}/{Date:yyyy/MM}/{Name|sanitize}.{Extension}
```

### Photo Organization

```
Photos/{Year}/{Month}/{Date:yyyy-MM-dd}_{Counter:000}.{Extension}
{Year}/{Date:MMM}/{Date:dd}_{Time:HHmmss}.{Extension}
Photos/{Year}/{Date:yyyy-MM-dd}/{Counter:0000}_{Name}.{Extension}
```

---

## Error Handling

### Invalid Templates

```csharp
try
{
    var result = engine.Process(template, context);
}
catch (TemplateException ex)
{
    Console.WriteLine($"Template error: {ex.Message}");
}
```

### Missing Variables

Missing variables resolve to empty strings:

```
Template: {Year}/{MissingVar}/{Name}
Result:   2025//Document
```

### Invalid Formats

Invalid format strings fall back to default:

```
{Date:INVALID_FORMAT}  ? 2025-01-15 (default format)
{Counter:INVALID}      ? 42 (no formatting)
```

---

## Performance

### Caching

Parse templates once and reuse:

```csharp
// Parse once
var validation = engine.Validate(template);
var parsedTemplate = validation.Template;

// Reuse many times
for (int i = 0; i < 1000; i++)
{
    var result = engine.Process(template, context);
}
```

### Benchmarks

- **Parse**: <1ms per template
- **Process**: <1ms per execution
- **Function**: <0.1ms per function call

---

## Troubleshooting

### Template Not Working

1. **Validate first**:
   ```csharp
   var validation = engine.Validate(template);
   ```

2. **Check variable names**: Case-insensitive but must match

3. **Check function names**: Must be registered

4. **Check brace matching**: All `{` must have matching `}`

### Unexpected Output

1. **Check variable values**: Print context values
2. **Check function behavior**: Test functions separately
3. **Check format strings**: Use valid .NET format strings

### Special Characters

Use `sanitize` function to handle special characters:

```
{Name|sanitize}  // Removes invalid filename characters
```

---

*Last Updated: January 3, 2025*  
*Version: 1.0*
