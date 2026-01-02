namespace DocsUnmessed.Services;

using DocsUnmessed.Core.Configuration;
using System.Text.Json;
using YamlDotNet.Serialization;

/// <summary>
/// Interface for loading configuration files
/// </summary>
public interface IConfigurationLoader
{
    Task<TiaBlueprint> LoadTiaBlueprintAsync(string path, CancellationToken cancellationToken = default);
    Task<List<MappingRule>> LoadMappingRulesAsync(string path, CancellationToken cancellationToken = default);
    Task<NamingTemplate> LoadNamingTemplateAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for loading configuration files (YAML, JSON)
/// </summary>
public sealed class ConfigurationLoader : IConfigurationLoader
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
    
    public async Task<TiaBlueprint> LoadTiaBlueprintAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"TIA blueprint file not found: {path}");
        }
        
        var yaml = await File.ReadAllTextAsync(path, cancellationToken);
        var deserializer = new DeserializerBuilder().Build();
        var blueprint = deserializer.Deserialize<TiaBlueprint>(yaml);
        
        return blueprint ?? throw new InvalidOperationException("Failed to load TIA blueprint");
    }
    
    public async Task<List<MappingRule>> LoadMappingRulesAsync(string path, CancellationToken cancellationToken = default)
    {
        // Check if path is a directory or file
        if (Directory.Exists(path))
        {
            // Load all JSON files from directory
            var rules = new List<MappingRule>();
            var files = Directory.GetFiles(path, "*.json");
            
            foreach (var file in files)
            {
                var rule = await LoadSingleMappingRuleAsync(file, cancellationToken);
                if (rule != null)
                {
                    rules.Add(rule);
                }
            }
            
            return rules;
        }
        else if (File.Exists(path))
        {
            // Load single file
            var rule = await LoadSingleMappingRuleAsync(path, cancellationToken);
            return rule != null ? new List<MappingRule> { rule } : new List<MappingRule>();
        }
        else
        {
            throw new FileNotFoundException($"Mapping rule path not found: {path}");
        }
    }
    
    public async Task<NamingTemplate> LoadNamingTemplateAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Naming template file not found: {path}");
        }
        
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var template = JsonSerializer.Deserialize<NamingTemplate>(json, _jsonOptions);
        
        return template ?? throw new InvalidOperationException("Failed to load naming template");
    }
    
    private async Task<MappingRule?> LoadSingleMappingRuleAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            return JsonSerializer.Deserialize<MappingRule>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load mapping rule from {path}: {ex.Message}");
            return null;
        }
    }
}
