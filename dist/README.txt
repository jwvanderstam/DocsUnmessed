DocsUnmessed GUI v1.0
======================

Privacy-first file organization tool for Windows

## What is DocsUnmessed?

DocsUnmessed helps you organize files across multiple storage locations:
- Local file system
- OneDrive (Personal & Business)
- Google Drive
- Dropbox
- iCloud Drive

## Features

? Scan directories with progress tracking
? Smart categorization (80+ file extensions, 8 categories)
? Preview migrations before executing (dry-run mode)
? Preserve folder structure
? Handle conflicts (rename, skip, overwrite)
? Multi-cloud support
? Privacy-first (all processing local)
? No telemetry

## How to Run

### Simple!
1. Extract this ZIP to any folder
2. Double-click: **DocsUnmessed.GUI.exe**
3. The application window will open

That's it! No installation required.

## System Requirements

- **Operating System**: Windows 10 (version 19041 or higher) or Windows 11
- **Memory**: 2 GB RAM minimum
- **Disk Space**: 200 MB free space
- **Display**: 1280x720 or higher recommended

**No .NET installation required!** Everything is included in the .exe file.

## Quick Start

### 1. Assess Your Files
- Click "Assess Files" on the Dashboard
- Select a directory to scan
- Choose provider (Local File System, OneDrive, etc.)
- Click "Start Scan"
- Wait for completion, note the Scan ID

### 2. Preview Migration
- Click "Migration" in the menu
- Enter the Scan ID from step 1
- Configure options:
  - Enable categorization (organize by file type)
  - Preserve structure (keep folder hierarchy)
- Enable "Dry-run mode" (preview only)
- Click "Preview"
- Review the changes

### 3. Execute Migration
- If satisfied with preview
- Disable "Dry-run mode"
- Click "Execute Migration"
- Watch progress
- Files are organized!

## Navigation

The application has 4 main pages:

1. **Dashboard** - Overview and quick actions
2. **Assess Files** - Scan directories
3. **Migration** - Organize and migrate files
4. **Settings** - Configure preferences

Use the ? menu (top-left) to navigate between pages.

## Configuration

### Default Settings
- Target directory: "migrated"
- Conflict resolution: Rename
- Categorization: Enabled
- Structure preservation: Enabled

### Customization
Go to Settings page to change defaults.

## File Categories

Files are organized into these categories:
- **Pictures** (.jpg, .png, .gif, .bmp, .svg, etc.)
- **Music** (.mp3, .wav, .flac, .aac, etc.)
- **Videos** (.mp4, .avi, .mkv, .mov, etc.)
- **Documents** (.pdf, .doc, .docx, .txt, .xlsx, etc.)
- **Archives** (.zip, .rar, .7z, .tar, etc.)
- **Code** (.cs, .js, .py, .java, etc.)
- **Executables** (.exe, .msi, .dll, etc.)
- **Other** (everything else)

## Safety Features

### Non-Destructive
- Files are **copied**, not moved (by default)
- Original files remain untouched
- Verify results before deleting originals

### Dry-Run Mode
- Preview changes without making them
- See exactly what will happen
- Adjust settings if needed

### Exclusion Filters
- System directories automatically excluded
- Configurable exclusion lists
- Prevents organizing system files

## Privacy & Security

- **No Telemetry**: We don't collect any data
- **Local Processing**: All operations happen on your computer
- **No Internet Required**: Works completely offline
- **Open Source**: Code is available for review

## Database

DocsUnmessed uses SQLite for storing scan results:
- File: `docsunmessed.db` (created in app directory)
- Contains: Scan history, file metadata
- Can be deleted to reset (no harm)

## Cloud Provider Setup

### OneDrive
Requires Microsoft Graph API credentials (for cloud access)
Or use local sync folder (no auth needed)

### Google Drive
Requires Google Drive API credentials
Or use local sync folder

### Dropbox
Requires Dropbox API credentials
Or use local sync folder

### iCloud Drive
Uses local sync folder (auto-detected)

**Note**: Local sync folder mode works without API credentials!

## Troubleshooting

### Application Won't Start
- Ensure Windows 10 version 19041 or higher
- Check Windows Defender didn't block it
- Run as Administrator (right-click > Run as administrator)

### Database Errors
- Delete `docsunmessed.db` file
- Restart application
- Database will be recreated

### Scan Errors
- Check file permissions
- Ensure directory exists
- Try excluding system directories

### Performance
- Scanning 10,000 files: ~2-5 seconds
- Larger scans: Progress is shown
- Timeout: 30 seconds per file (configurable)

## Support

### Documentation
See included documentation files:
- CREATE-EXECUTABLE-GUIDE.md - Build from source
- VISUAL-STUDIO-SETUP-GUIDE.md - Development guide
- CODE-QUALITY-VERIFICATION.md - Code quality report

### Issues
Report issues on GitHub: [Your Repository URL]

### Version
DocsUnmessed GUI v1.0
Built: 2025-01-15
.NET: 10.0

## License

MIT License

Copyright (c) 2025 DocsUnmessed

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Credits

Built with:
- .NET 10
- MAUI (Multi-platform App UI)
- Entity Framework Core
- SQLite
- CommunityToolkit.Mvvm

## Thank You!

Thank you for using DocsUnmessed!

We hope this tool helps you organize your files efficiently and securely.

---

DocsUnmessed - Privacy-first file organization
https://github.com/[YourUsername]/DocsUnmessed
