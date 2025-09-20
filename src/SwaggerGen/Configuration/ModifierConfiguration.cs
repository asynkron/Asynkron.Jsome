using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace SwaggerGen.Configuration;

/// <summary>
/// Root configuration class for controlling SwaggerGen object graph generation
/// </summary>
public class ModifierConfiguration
{
    /// <summary>
    /// Global settings that apply to the entire generation process
    /// </summary>
    [JsonProperty("global")]
    [YamlMember(Alias = "global")]
    public GlobalSettings? Global { get; set; }

    /// <summary>
    /// Map of property paths to their configuration rules.
    /// Key: Property path (e.g., "Order.OrderDetail.Product.Name")
    /// Value: Rule configuration for that property
    /// </summary>
    [JsonProperty("rules")]
    [YamlMember(Alias = "rules")]
    public Dictionary<string, PropertyRule> Rules { get; set; } = new();

    /// <summary>
    /// Gets the rule for a specific property path, or null if not configured
    /// </summary>
    /// <param name="propertyPath">The property path to look up</param>
    /// <returns>The property rule, or null if not found</returns>
    public PropertyRule? GetRule(string propertyPath)
    {
        return Rules.TryGetValue(propertyPath, out var rule) ? rule : null;
    }

    /// <summary>
    /// Determines if a property path should be included in generation
    /// </summary>
    /// <param name="propertyPath">The property path to check</param>
    /// <returns>True if the property should be included (default: true)</returns>
    public bool IsIncluded(string propertyPath)
    {
        var rule = GetRule(propertyPath);
        return rule?.IsIncluded ?? true;
    }

    /// <summary>
    /// Gets all rules that match or are children of the specified property path
    /// </summary>
    /// <param name="parentPath">The parent property path</param>
    /// <returns>Dictionary of matching rules</returns>
    public Dictionary<string, PropertyRule> GetChildRules(string parentPath)
    {
        var result = new Dictionary<string, PropertyRule>();
        var searchPrefix = parentPath + ".";
        
        foreach (var kvp in Rules)
        {
            if (kvp.Key.StartsWith(searchPrefix, StringComparison.OrdinalIgnoreCase))
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        
        return result;
    }
}

/// <summary>
/// Global settings that apply to the entire code generation process
/// </summary>
public class GlobalSettings
{
    /// <summary>
    /// Default namespace for generated code
    /// </summary>
    [JsonProperty("namespace")]
    [YamlMember(Alias = "namespace")]
    public string? Namespace { get; set; }

    /// <summary>
    /// Whether to generate enum types by default
    /// </summary>
    [JsonProperty("generateEnumTypes")]
    [YamlMember(Alias = "generateEnumTypes")]
    public bool? GenerateEnumTypes { get; set; }

    /// <summary>
    /// Default inclusion policy when no rule is specified
    /// </summary>
    [JsonProperty("defaultInclude")]
    [YamlMember(Alias = "defaultInclude")]
    public bool DefaultInclude { get; set; } = true;

    /// <summary>
    /// Whether to include descriptions from the original schema
    /// </summary>
    [JsonProperty("includeDescriptions")]
    [YamlMember(Alias = "includeDescriptions")]
    public bool IncludeDescriptions { get; set; } = true;

    /// <summary>
    /// Maximum depth for object graph traversal to prevent infinite recursion
    /// </summary>
    [JsonProperty("maxDepth")]
    [YamlMember(Alias = "maxDepth")]
    public int MaxDepth { get; set; } = 10;
}