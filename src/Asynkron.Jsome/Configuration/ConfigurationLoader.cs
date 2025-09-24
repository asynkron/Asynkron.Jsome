using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Asynkron.Jsome.Configuration;

/// <summary>
/// Utility class for loading modifier configurations from YAML or JSON files
/// </summary>
public static class ConfigurationLoader
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore
    };

    /// <summary>
    /// Loads a modifier configuration from a file path, auto-detecting the format based on extension
    /// </summary>
    /// <param name="filePath">Path to the configuration file (.yml, .yaml, or .json)</param>
    /// <returns>The loaded configuration</returns>
    /// <exception cref="ArgumentException">Thrown when file path is null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found</exception>
    /// <exception cref="NotSupportedException">Thrown when file extension is not supported</exception>
    public static async Task<ModifierConfiguration> LoadAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file not found: {filePath}");

        var content = await File.ReadAllTextAsync(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".yml" or ".yaml" => LoadFromYaml(content),
            ".json" => LoadFromJson(content),
            _ => throw new NotSupportedException($"Unsupported file extension: {extension}. Supported extensions are .yml, .yaml, and .json")
        };
    }

    /// <summary>
    /// Loads a modifier configuration from a file path synchronously
    /// </summary>
    /// <param name="filePath">Path to the configuration file (.yml, .yaml, or .json)</param>
    /// <returns>The loaded configuration</returns>
    public static ModifierConfiguration Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file not found: {filePath}");

        var content = File.ReadAllText(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".yml" or ".yaml" => LoadFromYaml(content),
            ".json" => LoadFromJson(content),
            _ => throw new NotSupportedException($"Unsupported file extension: {extension}. Supported extensions are .yml, .yaml, and .json")
        };
    }

    /// <summary>
    /// Loads a modifier configuration from a YAML string
    /// </summary>
    /// <param name="yamlContent">The YAML content</param>
    /// <returns>The loaded configuration</returns>
    public static ModifierConfiguration LoadFromYaml(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
            return new ModifierConfiguration();

        try
        {
            return YamlDeserializer.Deserialize<ModifierConfiguration>(yamlContent) ?? new ModifierConfiguration();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse YAML configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads a modifier configuration from a JSON string
    /// </summary>
    /// <param name="jsonContent">The JSON content</param>
    /// <returns>The loaded configuration</returns>
    public static ModifierConfiguration LoadFromJson(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return new ModifierConfiguration();

        try
        {
            return JsonConvert.DeserializeObject<ModifierConfiguration>(jsonContent, JsonSettings) ?? new ModifierConfiguration();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves a modifier configuration to a file, auto-detecting format based on extension
    /// </summary>
    /// <param name="configuration">The configuration to save</param>
    /// <param name="filePath">Path where to save the file (.yml, .yaml, or .json)</param>
    public static async Task SaveAsync(ModifierConfiguration configuration, string filePath)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var content = extension switch
        {
            ".yml" or ".yaml" => ToYaml(configuration),
            ".json" => ToJson(configuration),
            _ => throw new NotSupportedException($"Unsupported file extension: {extension}. Supported extensions are .yml, .yaml, and .json")
        };

        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    /// Converts a modifier configuration to YAML string
    /// </summary>
    /// <param name="configuration">The configuration to convert</param>
    /// <returns>YAML representation</returns>
    public static string ToYaml(ModifierConfiguration configuration)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(configuration);
    }

    /// <summary>
    /// Converts a modifier configuration to JSON string
    /// </summary>
    /// <param name="configuration">The configuration to convert</param>
    /// <returns>JSON representation</returns>
    public static string ToJson(ModifierConfiguration configuration)
    {
        return JsonConvert.SerializeObject(configuration, Formatting.Indented, JsonSettings);
    }
}