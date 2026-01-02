namespace DocsUnmessed.Tests.Unit.Helpers;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Factory for creating test Item instances
/// </summary>
public static class ItemFactory
{
    public static Item CreateTestItem(
        string path = "C:/Users/Test/Documents/test.txt",
        string name = "test.txt",
        string provider = "fs_local",
        long size = 1024,
        string mimeType = "text/plain",
        DateTime? createdUtc = null,
        DateTime? modifiedUtc = null,
        ItemType type = ItemType.File,
        int depth = 3)
    {
        return new Item
        {
            Path = path,
            Name = name,
            Provider = provider,
            Size = size,
            MimeType = mimeType,
            CreatedUtc = createdUtc ?? DateTime.UtcNow,
            ModifiedUtc = modifiedUtc ?? DateTime.UtcNow,
            Type = type,
            Depth = depth
        };
    }
    
    public static Item CreateOldPdfInDownloads(int daysOld = 100)
    {
        var modifiedDate = DateTime.UtcNow.AddDays(-daysOld);
        return CreateTestItem(
            path: "C:/Users/Test/Downloads/document.pdf",
            name: "document.pdf",
            mimeType: "application/pdf",
            createdUtc: modifiedDate,
            modifiedUtc: modifiedDate
        );
    }
    
    public static Item CreateImageFile(string extension = "jpg")
    {
        var name = $"photo.{extension}";
        return CreateTestItem(
            path: $"C:/Users/Test/Pictures/{name}",
            name: name,
            mimeType: $"image/{extension}",
            size: 2048
        );
    }
    
    public static Item CreateRecentDocument(string extension = "docx")
    {
        var name = $"document.{extension}";
        var recentDate = DateTime.UtcNow.AddDays(-5);
        return CreateTestItem(
            path: $"C:/Users/Test/Documents/{name}",
            name: name,
            mimeType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            createdUtc: recentDate,
            modifiedUtc: recentDate
        );
    }
}
