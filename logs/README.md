# Logs Directory

This directory contains application logs generated during DocsUnmessed operations.

## Log Files

- `docsunmessed-YYYYMMDD.log` - Daily application logs
- `audit-YYYYMMDD.jsonl` - Audit trail of file operations (JSON Lines format)
- `scan-<scan-id>.log` - Per-scan detailed logs
- `migration-<migration-id>.log` - Per-migration execution logs

## Log Levels

- **Debug**: Detailed diagnostic information
- **Info**: General informational messages
- **Warning**: Potentially harmful situations
- **Error**: Error events that might still allow the app to continue

## Audit Log Format

Each line in the audit log is a JSON object:

```json
{
  "timestamp": "2025-01-03T10:23:45Z",
  "operation": "copy",
  "source": "C:\\Users\\Me\\Downloads\\file.pdf",
  "target": "C:\\OneDrive\\03_Tech\\99_Archive\\file.pdf",
  "status": "completed",
  "hashMatch": true,
  "correlationId": "abc123def456"
}
```

## Retention

Logs are retained for 90 days by default. Configure retention in `appsettings.json`.

## Privacy

Logs may contain file paths and names. Store logs securely and do not share publicly if they contain sensitive information.

---

**Note**: This directory is excluded from version control (.gitignore).
