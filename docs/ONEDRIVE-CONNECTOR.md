# OneDrive Connector Guide

## Overview

The OneDrive connector provides seamless integration with Microsoft OneDrive using the Microsoft Graph API. It supports authentication, file operations, progress tracking, and includes built-in rate limiting and retry policies.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Azure AD App Setup](#azure-ad-app-setup)
3. [Quick Start](#quick-start)
4. [Configuration](#configuration)
5. [Authentication](#authentication)
6. [File Operations](#file-operations)
7. [Advanced Features](#advanced-features)
8. [Error Handling](#error-handling)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Requirements
- .NET 10 SDK
- Azure AD tenant (or Microsoft Account)
- Registered Azure AD application
- Microsoft Graph API access

### NuGet Packages
```xml
<PackageReference Include="System.Net.Http.Json" Version="9.0.0" />
```

---

## Azure AD App Setup

### Step 1: Register Application

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Fill in details:
   - **Name**: DocsUnmessed OneDrive Connector
   - **Supported account types**: Accounts in any organizational directory and personal Microsoft accounts
   - **Redirect URI**: `http://localhost:8080` (for development)

### Step 2: Configure Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph** > **Delegated permissions**
4. Add these permissions:
   - `Files.Read`
   - `Files.ReadWrite`
   - `Files.Read.All`
   - `Files.ReadWrite.All`
   - `User.Read`
5. Click **Grant admin consent**

### Step 3: Get Client ID

1. Go to **Overview**
2. Copy **Application (client) ID**
3. Save for configuration

### Step 4: Authentication Setup (Optional)

For production apps:
1. Go to **Certificates & secrets**
2. Create a new **Client secret**
3. Save the value (shown only once)

---

## Quick Start

### Basic Usage

```csharp
using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.OneDrive;

// 1. Configure
var config = new OneDriveConfig
{
    ClientId = "your-client-id-here"
};

// 2. Create connector
using var connector = new OneDriveConnector(config);

// 3. Authenticate (you need to obtain access token separately)
var credentials = new CloudCredentials
{
    Type = AuthenticationType.OAuth2,
    AccessToken = "your-access-token"
};

var authResult = await connector.AuthenticateAsync(credentials);
if (!authResult.IsSuccess)
{
    Console.WriteLine($"Authentication failed: {authResult.ErrorMessage}");
    return;
}

// 4. List files
var items = await connector.ListItemsAsync("/Documents");
foreach (var item in items)
{
    Console.WriteLine($"{item.Name} - {item.Size} bytes");
}
```

### Getting an Access Token

#### Option 1: Using Azure.Identity (Recommended)

```csharp
using Azure.Identity;

var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
{
    ClientId = "your-client-id",
    TenantId = "common",
    RedirectUri = new Uri("http://localhost:8080")
});

var tokenRequestContext = new TokenRequestContext(new[]
{
    "https://graph.microsoft.com/Files.ReadWrite",
    "https://graph.microsoft.com/User.Read"
});

var token = await credential.GetTokenAsync(tokenRequestContext);
var accessToken = token.Token;
```

#### Option 2: Using MSAL.NET

```csharp
using Microsoft.Identity.Client;

var app = PublicClientApplicationBuilder
    .Create("your-client-id")
    .WithRedirectUri("http://localhost:8080")
    .Build();

var scopes = new[]
{
    "https://graph.microsoft.com/Files.ReadWrite",
    "https://graph.microsoft.com/User.Read"
};

var result = await app.AcquireTokenInteractive(scopes)
    .ExecuteAsync();

var accessToken = result.AccessToken;
```

---

## Configuration

### OneDriveConfig Properties

```csharp
var config = new OneDriveConfig
{
    // Required
    ClientId = "your-client-id",
    
    // Optional (with defaults)
    GraphApiBaseUrl = "https://graph.microsoft.com/v1.0", // Graph API endpoint
    TenantId = "common",                                   // Tenant or "common"
    RedirectUri = "http://localhost:8080",                 // OAuth redirect
    UploadChunkSize = 320 * 1024,                         // 320KB chunks
    UseDeltaQueries = true,                                // Use delta queries
    MaxPageSize = 200                                      // Items per page
};
```

### Configuration Examples

#### Single Tenant

```csharp
var config = new OneDriveConfig
{
    ClientId = "your-client-id",
    TenantId = "your-tenant-id" // Specific tenant
};
```

#### Multi-Tenant

```csharp
var config = new OneDriveConfig
{
    ClientId = "your-client-id",
    TenantId = "common" // Any Microsoft account
};
```

#### Custom Chunk Size

```csharp
var config = new OneDriveConfig
{
    ClientId = "your-client-id",
    UploadChunkSize = 5 * 1024 * 1024 // 5MB chunks for faster uploads
};
```

---

## Authentication

### Basic Authentication

```csharp
var credentials = new CloudCredentials
{
    Type = AuthenticationType.OAuth2,
    AccessToken = "your-access-token"
};

var result = await connector.AuthenticateAsync(credentials);

if (result.IsSuccess)
{
    Console.WriteLine($"Authenticated as: {result.UserInfo?.DisplayName}");
    Console.WriteLine($"Email: {result.UserInfo?.Email}");
    
    if (result.UserInfo?.Quota != null)
    {
        var quota = result.UserInfo.Quota;
        Console.WriteLine($"Storage: {quota.UsedBytes:N0} / {quota.TotalBytes:N0} bytes");
        Console.WriteLine($"Available: {quota.AvailableBytes:N0} bytes");
    }
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### Check Authentication Status

```csharp
if (connector.IsAuthenticated)
{
    // Proceed with operations
}
else
{
    // Re-authenticate
}
```

---

## File Operations

### List Files

#### List Root Items

```csharp
var items = await connector.ListItemsAsync("/");

foreach (var item in items)
{
    Console.WriteLine($"{item.Name} ({item.Type})");
}
```

#### List Specific Folder

```csharp
var items = await connector.ListItemsAsync("/Documents");
```

#### Recursive Listing

```csharp
var items = await connector.ListItemsAsync("/Photos", recursive: true);

Console.WriteLine($"Found {items.Count} items");
foreach (var item in items.Where(i => i.Type == ItemType.File))
{
    Console.WriteLine($"{item.Path} - {item.Size:N0} bytes");
}
```

### Download Files

#### Simple Download

```csharp
await connector.DownloadFileAsync(
    remotePath: "/Documents/report.pdf",
    localPath: "C:/Downloads/report.pdf"
);
```

#### Download with Progress

```csharp
var progress = new Progress<TransferProgress>(p =>
{
    Console.WriteLine($"Progress: {p.PercentComplete:F1}%");
    Console.WriteLine($"Downloaded: {p.TransferredBytes:N0} / {p.TotalBytes:N0} bytes");
});

await connector.DownloadFileAsync(
    remotePath: "/Documents/large-file.zip",
    localPath: "C:/Downloads/large-file.zip",
    progress: progress
);
```

#### Download with Progress Bar

```csharp
var lastPercent = 0.0;
var progress = new Progress<TransferProgress>(p =>
{
    if (p.PercentComplete - lastPercent >= 1.0)
    {
        var bar = new string('?', (int)(p.PercentComplete / 2));
        Console.Write($"\r[{bar,-50}] {p.PercentComplete:F1}%");
        lastPercent = p.PercentComplete;
    }
});

await connector.DownloadFileAsync(remotePath, localPath, progress);
Console.WriteLine("\nDownload complete!");
```

### Upload Files

#### Small File Upload

```csharp
await connector.UploadFileAsync(
    localPath: "C:/Documents/report.docx",
    remotePath: "/Work/report.docx"
);
```

#### Large File Upload (Chunked)

```csharp
// Files larger than 3.2MB automatically use chunked upload
await connector.UploadFileAsync(
    localPath: "C:/Videos/presentation.mp4",
    remotePath: "/Presentations/demo.mp4"
);
```

#### Upload with Progress

```csharp
var progress = new Progress<TransferProgress>(p =>
{
    Console.WriteLine($"Uploaded: {p.TransferredBytes:N0} / {p.TotalBytes:N0} bytes");
    Console.WriteLine($"Progress: {p.PercentComplete:F1}%");
});

await connector.UploadFileAsync(localPath, remotePath, progress);
```

### Delete Files

```csharp
await connector.DeleteAsync("/Documents/old-file.txt");
```

### Get Item Metadata

```csharp
var item = await connector.GetItemAsync("/Documents/report.pdf");

if (item != null)
{
    Console.WriteLine($"Name: {item.Name}");
    Console.WriteLine($"Size: {item.Size:N0} bytes");
    Console.WriteLine($"Modified: {item.ModifiedUtc}");
    Console.WriteLine($"Hash: {item.Hash}");
    Console.WriteLine($"Shared: {item.IsShared}");
    
    // OneDrive-specific properties
    if (item.ExtendedProperties.TryGetValue("OneDriveId", out var id))
    {
        Console.WriteLine($"OneDrive ID: {id}");
    }
}
```

---

## Advanced Features

### Rate Limiting

```csharp
// Conservative rate limiting
var rateLimitConfig = new RateLimitConfig
{
    MaxRequests = 50,
    TimeWindow = TimeSpan.FromMinutes(1)
};

var connector = new OneDriveConnector(config, rateLimitConfig);
```

### Retry Policies

```csharp
// Aggressive retry for unreliable networks
var retryPolicy = new RetryPolicy(
    maxRetries: 5,
    initialDelay: TimeSpan.FromMilliseconds(500),
    backoffMultiplier: 1.5
);

var connector = new OneDriveConnector(config, retryPolicy: retryPolicy);
```

### Cancellation Support

```csharp
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5));

try
{
    var items = await connector.ListItemsAsync("/", cancellationToken: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out");
}
```

### Batch Operations

```csharp
var filesToUpload = new[]
{
    ("local1.txt", "/remote1.txt"),
    ("local2.txt", "/remote2.txt"),
    ("local3.txt", "/remote3.txt")
};

var tasks = filesToUpload.Select(f =>
    connector.UploadFileAsync(f.Item1, f.Item2));

await Task.WhenAll(tasks);
```

---

## Error Handling

### Common Errors

#### Not Authenticated

```csharp
try
{
    await connector.ListItemsAsync("/");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Not authenticated. Please authenticate first.");
}
```

#### File Not Found

```csharp
try
{
    await connector.DownloadFileAsync("/missing.txt", "local.txt");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
```

#### Network Errors

```csharp
try
{
    await connector.ListItemsAsync("/");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
    // Retry logic handled automatically if retry policy configured
}
```

### Comprehensive Error Handling

```csharp
try
{
    var items = await connector.ListItemsAsync("/Documents");
    Console.WriteLine($"Found {items.Count} items");
}
catch (InvalidOperationException)
{
    Console.WriteLine("Not authenticated");
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine("Path not found");
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    Console.WriteLine("Token expired - re-authenticate");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP error: {ex.Message}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation cancelled");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

---

## Best Practices

### 1. Token Management

```csharp
// Store tokens securely
var tokenCache = new SecureTokenCache();
var accessToken = await tokenCache.GetTokenAsync();

// Refresh tokens before expiry
if (authResult.ExpiresAt < DateTime.UtcNow.AddMinutes(5))
{
    accessToken = await RefreshTokenAsync();
}
```

### 2. Quota Monitoring

```csharp
var authResult = await connector.AuthenticateAsync(credentials);
if (authResult.UserInfo?.Quota != null)
{
    var quota = authResult.UserInfo.Quota;
    var percentUsed = (quota.UsedBytes / (double)quota.TotalBytes) * 100;
    
    if (percentUsed > 90)
    {
        Console.WriteLine($"Warning: {percentUsed:F1}% of quota used");
    }
}
```

### 3. Efficient Listing

```csharp
// Use non-recursive for large folders
var items = await connector.ListItemsAsync("/Photos", recursive: false);

// Filter in code if needed
var recentFiles = items
    .Where(i => i.Type == ItemType.File)
    .Where(i => i.ModifiedUtc > DateTime.UtcNow.AddDays(-30));
```

### 4. Progress Feedback

```csharp
// Always provide progress for large files
var progress = new Progress<TransferProgress>(p =>
{
    if (p.TotalBytes > 10 * 1024 * 1024) // > 10MB
    {
        Console.WriteLine($"Progress: {p.PercentComplete:F1}%");
    }
});
```

### 5. Dispose Properly

```csharp
// Use using statement
using (var connector = new OneDriveConnector(config))
{
    // Use connector
}

// Or explicit disposal
var connector = new OneDriveConnector(config);
try
{
    // Use connector
}
finally
{
    connector.Dispose();
}
```

---

## Troubleshooting

### Issue: "Access token is required"

**Cause**: No access token provided  
**Solution**: Obtain access token using Azure.Identity or MSAL.NET

```csharp
// Make sure token is not null or empty
if (string.IsNullOrEmpty(accessToken))
{
    Console.WriteLine("Please authenticate to get access token");
}
```

### Issue: "Not authenticated. Call AuthenticateAsync first"

**Cause**: Attempting operations before authentication  
**Solution**: Call AuthenticateAsync before other operations

```csharp
if (!connector.IsAuthenticated)
{
    await connector.AuthenticateAsync(credentials);
}
```

### Issue: "Authentication failed: Unauthorized"

**Cause**: Invalid or expired token  
**Solution**: Get a new token

```csharp
// Token might be expired
var newToken = await RefreshTokenAsync();
var credentials = new CloudCredentials
{
    Type = AuthenticationType.OAuth2,
    AccessToken = newToken
};
await connector.AuthenticateAsync(credentials);
```

### Issue: Slow uploads

**Cause**: Default chunk size too small  
**Solution**: Increase chunk size

```csharp
var config = new OneDriveConfig
{
    ClientId = "your-client-id",
    UploadChunkSize = 10 * 1024 * 1024 // 10MB chunks
};
```

### Issue: Rate limiting errors

**Cause**: Too many requests  
**Solution**: Configure rate limiting

```csharp
var rateLimitConfig = RateLimitConfig.Conservative;
var connector = new OneDriveConnector(config, rateLimitConfig);
```

### Issue: Network timeouts

**Cause**: Slow connection  
**Solution**: Configure retry policy

```csharp
var retryPolicy = new RetryPolicy(
    maxRetries: 5,
    initialDelay: TimeSpan.FromSeconds(2)
);
var connector = new OneDriveConnector(config, retryPolicy: retryPolicy);
```

---

## Complete Example

```csharp
using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.OneDrive;
using Azure.Identity;

public class OneDriveExample
{
    public static async Task Main(string[] args)
    {
        // 1. Configure
        var config = new OneDriveConfig
        {
            ClientId = "your-client-id",
            UploadChunkSize = 5 * 1024 * 1024 // 5MB chunks
        };

        // 2. Get access token
        var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            ClientId = config.ClientId,
            TenantId = "common"
        });

        var tokenContext = new TokenRequestContext(new[]
        {
            "https://graph.microsoft.com/Files.ReadWrite",
            "https://graph.microsoft.com/User.Read"
        });

        var token = await credential.GetTokenAsync(tokenContext);

        // 3. Create connector
        using var connector = new OneDriveConnector(config);

        // 4. Authenticate
        var credentials = new CloudCredentials
        {
            Type = AuthenticationType.OAuth2,
            AccessToken = token.Token
        };

        var authResult = await connector.AuthenticateAsync(credentials);
        if (!authResult.IsSuccess)
        {
            Console.WriteLine($"Auth failed: {authResult.ErrorMessage}");
            return;
        }

        Console.WriteLine($"? Authenticated as: {authResult.UserInfo?.DisplayName}");

        // 5. List files
        Console.WriteLine("\nListing files in /Documents...");
        var items = await connector.ListItemsAsync("/Documents");
        
        foreach (var item in items)
        {
            var icon = item.Type == ItemType.Folder ? "??" : "??";
            Console.WriteLine($"{icon} {item.Name} ({item.Size:N0} bytes)");
        }

        // 6. Download a file with progress
        Console.WriteLine("\nDownloading file...");
        var progress = new Progress<TransferProgress>(p =>
        {
            var bar = new string('?', (int)(p.PercentComplete / 2));
            Console.Write($"\r[{bar,-50}] {p.PercentComplete:F1}%");
        });

        await connector.DownloadFileAsync(
            "/Documents/report.pdf",
            "C:/Downloads/report.pdf",
            progress
        );

        Console.WriteLine("\n? Download complete!");

        // 7. Upload a file
        Console.WriteLine("\nUploading file...");
        await connector.UploadFileAsync(
            "C:/Documents/new-report.docx",
            "/Documents/new-report.docx",
            progress
        );

        Console.WriteLine("\n? Upload complete!");
        Console.WriteLine("\nAll operations completed successfully!");
    }
}
```

---

## API Reference

### OneDriveConnector Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `AuthenticateAsync(credentials)` | Authenticate with OneDrive | `AuthenticationResult` |
| `ListItemsAsync(path, recursive)` | List items in path | `IReadOnlyList<Item>` |
| `DownloadFileAsync(remotePath, localPath, progress)` | Download file | `Task` |
| `UploadFileAsync(localPath, remotePath, progress)` | Upload file | `Task` |
| `DeleteAsync(path)` | Delete file or folder | `Task` |
| `GetItemAsync(path)` | Get item metadata | `Item?` |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ProviderName` | `string` | Always "OneDrive" |
| `IsAuthenticated` | `bool` | Authentication status |

---

## Resources

- [Microsoft Graph API Documentation](https://docs.microsoft.com/graph/)
- [Azure AD App Registration](https://portal.azure.com/)
- [Microsoft Graph Explorer](https://developer.microsoft.com/graph/graph-explorer)
- [MSAL.NET Documentation](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)

---

*Last Updated: January 2025*  
*Version: 1.0*  
*DocsUnmessed Phase 3*
