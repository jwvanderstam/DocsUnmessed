# Cloud Connectors Guide

## Overview

DocsUnmessed provides a robust cloud connector framework for integrating with various cloud storage providers. The framework includes built-in rate limiting, retry policies, and progress tracking.

---

## Architecture

### Core Components

```
ICloudConnector (Interface)
    ?
CloudConnectorBase (Abstract Base)
    ?
    ?? RateLimiter (Rate Limiting)
    ?? RetryPolicy (Error Retry)
    ?? Progress Tracking
```

### Key Features

? **Rate Limiting** - Automatic throttling to respect API limits  
? **Retry Logic** - Exponential backoff for transient errors  
? **Progress Tracking** - Real-time transfer progress callbacks  
? **Authentication** - OAuth 2.0, API Key, and Basic auth support  
? **Quota Management** - Storage quota monitoring  
? **Thread Safety** - Concurrent operation support  

---

## Quick Start

### 1. Basic Usage

```csharp
using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;

// Create connector with default settings
using var connector = new YourCloudConnector();

// Authenticate
var credentials = new CloudCredentials
{
    Type = AuthenticationType.OAuth2,
    AccessToken = "your-access-token"
};

var authResult = await connector.AuthenticateAsync(credentials);
if (!authResult.IsSuccess)
{
    Console.WriteLine($"Auth failed: {authResult.ErrorMessage}");
    return;
}

// List files
var items = await connector.ListItemsAsync("/Documents");
foreach (var item in items)
{
    Console.WriteLine($"{item.Name} - {item.Size} bytes");
}
```

### 2. With Custom Configuration

```csharp
// Configure rate limiting
var rateLimitConfig = new RateLimitConfig
{
    MaxRequests = 50,  // 50 requests
    TimeWindow = TimeSpan.FromMinutes(1)  // per minute
};

// Configure retry policy
var retryPolicy = new RetryPolicy(
    maxRetries: 3,
    initialDelay: TimeSpan.FromSeconds(1),
    backoffMultiplier: 2.0
);

// Create connector with custom settings
using var connector = new YourCloudConnector(rateLimitConfig, retryPolicy);
```

---

## Implementing a Cloud Connector

### Step 1: Create Connector Class

```csharp
using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Core.Domain;

public sealed class MyCloudConnector : CloudConnectorBase
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;

    public MyCloudConnector(
        RateLimitConfig? rateLimitConfig = null,
        RetryPolicy? retryPolicy = null)
        : base(rateLimitConfig, retryPolicy)
    {
        _httpClient = new HttpClient();
    }

    public override string ProviderName => "MyCloud";

    public override bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

    // Implement required methods...
}
```

### Step 2: Implement Authentication

```csharp
public override async Task<AuthenticationResult> AuthenticateAsync(
    CloudCredentials credentials,
    CancellationToken cancellationToken = default)
{
    return await ExecuteApiCallAsync(async () =>
    {
        // Call your authentication API
        var response = await _httpClient.PostAsync(
            "https://api.mycloud.com/auth",
            new StringContent(JsonSerializer.Serialize(new
            {
                grant_type = "authorization_code",
                code = credentials.AccessToken
            })),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = $"Auth failed: {response.StatusCode}"
            };
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var authData = JsonSerializer.Deserialize<AuthResponse>(json);
        
        _accessToken = authData.AccessToken;

        return new AuthenticationResult
        {
            IsSuccess = true,
            UserInfo = new CloudUserInfo
            {
                UserId = authData.UserId,
                Email = authData.Email,
                DisplayName = authData.Name,
                Quota = new QuotaInfo
                {
                    TotalBytes = authData.Quota.Total,
                    UsedBytes = authData.Quota.Used
                }
            },
            ExpiresAt = DateTime.UtcNow.AddSeconds(authData.ExpiresIn)
        };
    }, cancellationToken);
}
```

### Step 3: Implement File Operations

```csharp
public override async Task<IReadOnlyList<Item>> ListItemsAsync(
    string path,
    bool recursive = false,
    CancellationToken cancellationToken = default)
{
    return await ExecuteApiCallAsync(async () =>
    {
        var items = new List<Item>();
        
        var response = await _httpClient.GetAsync(
            $"https://api.mycloud.com/files?path={path}&recursive={recursive}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var files = JsonSerializer.Deserialize<FileListResponse>(json);

        foreach (var file in files.Items)
        {
            items.Add(new Item
            {
                Path = file.Path,
                Name = file.Name,
                Provider = ProviderName,
                Size = file.Size,
                Type = file.IsFolder ? ItemType.Folder : ItemType.File,
                MimeType = file.MimeType,
                CreatedUtc = file.CreatedAt,
                ModifiedUtc = file.ModifiedAt,
                Hash = file.Hash,
                ExtendedProperties = new Dictionary<string, string>(),
                Issues = new List<string>(),
                IsShared = file.IsShared,
                Depth = path.Split('/').Length
            });
        }

        return items;
    }, cancellationToken);
}
```

---

## Rate Limiting

### Overview

The rate limiter uses a sliding window algorithm to respect API limits.

### Configuration

```csharp
var config = new RateLimitConfig
{
    MaxRequests = 100,  // Maximum requests
    TimeWindow = TimeSpan.FromMinutes(1)  // Time window
};
```

### Presets

```csharp
// Default: 100 requests per minute
var config = RateLimitConfig.Default;

// Conservative: 50 requests per minute
var config = RateLimitConfig.Conservative;

// Aggressive: 200 requests per minute
var config = RateLimitConfig.Aggressive;
```

### How It Works

1. Tracks timestamps of recent API calls
2. Enforces maximum requests within time window
3. Automatically delays requests if limit reached
4. Thread-safe for concurrent operations

---

## Retry Policies

### Overview

Retry policies automatically retry failed operations with exponential backoff.

### Configuration

```csharp
var policy = new RetryPolicy(
    maxRetries: 3,  // Number of retries
    initialDelay: TimeSpan.FromSeconds(1),  // First delay
    backoffMultiplier: 2.0,  // Delay multiplier
    maxDelay: TimeSpan.FromMinutes(5)  // Maximum delay
);
```

### Presets

```csharp
// Default: 3 retries with 1s initial delay
var policy = RetryPolicy.Default;

// No retries
var policy = RetryPolicy.None;

// Aggressive: 5 retries with 500ms initial delay
var policy = RetryPolicy.Aggressive;
```

### Transient Errors

Automatically retries these error types:
- `HttpRequestException` - Network errors
- `TimeoutException` - Request timeouts
- `TaskCanceledException` - Task cancellations
- `IOException` - Network-related I/O errors

### Backoff Example

```
Attempt 1: Immediate
Attempt 2: Wait 1s
Attempt 3: Wait 2s
Attempt 4: Wait 4s (exponential)
```

---

## Progress Tracking

### Download with Progress

```csharp
var progress = new Progress<TransferProgress>(p =>
{
    Console.WriteLine($"Downloaded: {p.TransferredBytes:N0} / {p.TotalBytes:N0} bytes");
    Console.WriteLine($"Progress: {p.PercentComplete:F1}%");
    Console.WriteLine($"Speed: {p.BytesPerSecond / 1024:F1} KB/s");
});

await connector.DownloadFileAsync(
    remotePath: "/Documents/file.pdf",
    localPath: "C:/Downloads/file.pdf",
    progress: progress
);
```

### Upload with Progress

```csharp
var progress = new Progress<TransferProgress>(p =>
{
    var progressBar = new string('?', (int)(p.PercentComplete / 2));
    Console.Write($"\r[{progressBar,-50}] {p.PercentComplete:F1}%");
});

await connector.UploadFileAsync(
    localPath: "C:/Documents/file.pdf",
    remotePath: "/Backup/file.pdf",
    progress: progress
);
```

---

## Error Handling

### Basic Error Handling

```csharp
try
{
    var items = await connector.ListItemsAsync("/Documents");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (RetryExhaustedException ex)
{
    Console.WriteLine($"All retries failed: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Checking Authentication

```csharp
if (!connector.IsAuthenticated)
{
    Console.WriteLine("Not authenticated. Please authenticate first.");
    return;
}
```

---

## Best Practices

### 1. Use Appropriate Rate Limits

```csharp
// For API providers with strict limits
var config = RateLimitConfig.Conservative;

// For providers with generous limits
var config = RateLimitConfig.Aggressive;
```

### 2. Handle Quota Limits

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

### 3. Dispose Properly

```csharp
// Use using statement
using var connector = new MyCloudConnector();

// Or explicit disposal
var connector = new MyCloudConnector();
try
{
    // Use connector
}
finally
{
    connector.Dispose();
}
```

### 4. Use Cancellation Tokens

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

---

## Testing

### Mock Connector for Testing

```csharp
using var mockConnector = new MockCloudConnector();

// Simulate failures for testing retry logic
mockConnector.SimulateFailures = true;
mockConnector.FailuresBeforeSuccess = 2;

// Should retry and succeed
var items = await mockConnector.ListItemsAsync("/test");
```

### Testing Rate Limiting

```csharp
var config = new RateLimitConfig
{
    MaxRequests = 5,
    TimeWindow = TimeSpan.FromSeconds(1)
};

using var connector = new MockCloudConnector(rateLimitConfig: config);

// Make 10 requests - should be throttled
for (int i = 0; i < 10; i++)
{
    await connector.ListItemsAsync("/test");
}
```

---

## Performance Tips

### 1. Batch Operations

```csharp
// Good: Batch multiple files
var files = await connector.ListItemsAsync("/Documents");
foreach (var file in files)
{
    // Process
}

// Avoid: Individual API calls
for (int i = 0; i < 100; i++)
{
    var file = await connector.GetItemAsync($"/Documents/file{i}.txt");
}
```

### 2. Reuse Connectors

```csharp
// Good: Reuse connector
using var connector = new MyCloudConnector();
await connector.AuthenticateAsync(credentials);

for (int i = 0; i < 100; i++)
{
    await connector.GetItemAsync($"/file{i}");
}

// Avoid: Creating new connectors
for (int i = 0; i < 100; i++)
{
    using var conn = new MyCloudConnector();
    await conn.AuthenticateAsync(credentials);
    await conn.GetItemAsync($"/file{i}");
}
```

### 3. Parallel Operations

```csharp
var tasks = files.Select(file => 
    connector.DownloadFileAsync(file.Path, $"C:/Downloads/{file.Name}"));

await Task.WhenAll(tasks);
```

---

## Troubleshooting

### Issue: Rate Limit Exceeded

**Solution**: Reduce `MaxRequests` or increase `TimeWindow`:
```csharp
var config = new RateLimitConfig
{
    MaxRequests = 50,  // Reduce
    TimeWindow = TimeSpan.FromMinutes(2)  // Increase
};
```

### Issue: Retries Exhausted

**Solution**: Increase retry attempts or check network:
```csharp
var policy = new RetryPolicy(
    maxRetries: 5,  // Increase
    initialDelay: TimeSpan.FromSeconds(2)
);
```

### Issue: Authentication Fails

**Solution**: Check credentials and token expiration:
```csharp
if (authResult.ExpiresAt < DateTime.UtcNow)
{
    // Token expired, re-authenticate
    authResult = await connector.AuthenticateAsync(newCredentials);
}
```

---

## Example: Complete Implementation

See `tests/DocsUnmessed.Tests.Integration/Mocks/MockCloudConnector.cs` for a complete implementation example.

---

*Last Updated: January 2025*  
*Version: 1.0*
