# DocsUnmessed ??

> A powerful document organization tool for Windows with smart categorization, duplicate detection, and cloud integration.

[![.NET 10](https://img.shields.io/badge/.NET-10-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-256%2F259%20passing-brightgreen)](tests/)

## ? Features

- ?? **Smart File Scanning** - Analyze local and cloud storage
- ?? **Intelligent Migration** - Rules-based file organization
- ?? **Cloud Integration** - OneDrive, Google Drive, Dropbox support
- ?? **Duplicate Detection** - Find and manage duplicate files
- ?? **Statistics & Reports** - Detailed analysis of your files
- ?? **Native Windows GUI** - Clean WPF interface
- ?? **Powerful CLI** - Full command-line functionality

## ?? Quick Start

### Run the GUI
```powershell
dotnet run --project src\GUI\DocsUnmessed.GUI.csproj
```

### Use the CLI
```powershell
# Scan files
dotnet run -- assess --root "C:\Users\Me\Documents" --providers fs_local

# Preview migration
dotnet run -- migrate --scan-id <scan-id> --dry-run

# Execute migration
dotnet run -- migrate --scan-id <scan-id> --categorize
```

## ?? Project Structure

```
DocsUnmessed/
??? src/
?   ??? GUI/              # Native Windows WPF application
?   ??? CLI/              # Command-line interface
?   ??? Core/             # Domain models and rules
?   ??? Data/             # Database and repositories
?   ??? Services/         # Business logic
?   ??? Connectors/       # File system and cloud connectors
??? tests/
?   ??? DocsUnmessed.Tests.Unit/          # 84 unit tests
?   ??? DocsUnmessed.Tests.Integration/   # 172 integration tests
??? docs/                 # Documentation
?   ??? guides/           # User guides
?   ??? build/            # Build instructions
?   ??? development/      # Development docs
?   ??? architecture/     # Architecture docs
??? scripts/              # Build and utility scripts
??? examples/             # Configuration examples
??? _archive/             # Archived code
```

## ?? Documentation

### Getting Started
- [Quick Start Guide](docs/guides/QUICK-START-CARD.md)
- [Visual Studio Setup](docs/guides/VISUAL-STUDIO-SETUP-GUIDE.md)
- [CLI Reference](docs/CLI-REFERENCE.md)

### Building & Deployment
- [Build Guide](docs/build/CREATE-EXECUTABLE-GUIDE.md)
- [Quick Build](docs/build/CREATE-EXECUTABLE-QUICKSTART.md)
- [Visual Studio Publishing](docs/build/VS-PUBLISH-QUICK-CARD.md)

### Development
- [Architecture](docs/architecture/SOLUTION-STRUCTURE.md)
- [Code Quality](docs/development/CODE-QUALITY-VERIFICATION.md)
- [Project Standards](docs/PROJECT-STANDARDS.md)

### Features & Guides
- [GUI Integration](docs/guides/GUI-CLI-INTEGRATION.md)
- [Cloud Connectors](docs/CLOUD-CONNECTORS.md)
- [Migration Guide](docs/MIGRATION-GUIDE.md)
- [Naming Templates](docs/NAMING-TEMPLATES.md)

## ??? Building

### Prerequisites
- .NET 10 SDK
- Windows 10/11

### Build Commands
```powershell
# Build main library
dotnet build DocsUnmessed.csproj

# Build GUI
dotnet build src\GUI\DocsUnmessed.GUI.csproj

# Build executable
.\scripts\build-gui-executable.ps1

# Run tests
dotnet test
```

## ?? Testing

```powershell
# All tests
dotnet test

# Unit tests only (84 tests)
dotnet test tests\DocsUnmessed.Tests.Unit

# Integration tests only (172 tests)
dotnet test tests\DocsUnmessed.Tests.Integration
```

**Test Status**: ? 256/259 passing (99%)

## ?? Key Components

### GUI (WPF)
- Dashboard with statistics
- Assess page for scanning
- Migration page for organizing
- Settings and configuration

### CLI Commands
- `assess` - Scan and analyze files
- `migrate` - Execute migration plans
- `simulate` - Preview changes (dry-run)
- `validate` - Check rule configurations

### Rules Engine
- Regex path matching
- Extension-based rules
- Age-based categorization
- Composite rule logic

### Cloud Connectors
- Local file system ?
- OneDrive ?
- Google Drive (in progress)
- Dropbox (in progress)

## ?? Project Status

| Component | Status |
|-----------|--------|
| Core Library | ? Complete |
| CLI | ? Complete |
| GUI (WPF) | ? Complete |
| Unit Tests | ? 84/84 (100%) |
| Integration Tests | ? 172/175 (98%) |
| Documentation | ? Complete |

## ?? Contributing

Contributions are welcome! Please read our [Project Standards](docs/PROJECT-STANDARDS.md) first.

## ?? License

This project is licensed under the MIT License.

## ?? Acknowledgments

Built with:
- .NET 10
- Entity Framework Core
- WPF
- CommunityToolkit.Mvvm
- NUnit

---

**Status**: Production Ready ?  
**Version**: 1.0.0  
**Last Updated**: January 2025
