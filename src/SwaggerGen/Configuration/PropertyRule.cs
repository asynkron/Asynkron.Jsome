using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace SwaggerGen.Configuration;

/// <summary>
/// Represents a rule that can be applied to a specific property path in the object graph
/// </summary>
public class PropertyRule
{
    /// <summary>
    /// Whether this property should be included in the generated object graph.
    /// If null, defaults to true (included by default).
    /// </summary>
    [JsonProperty("include")]
    [YamlMember(Alias = "include")]
    public bool? Include { get; set; }

    /// <summary>
    /// Custom formatting options for the property
    /// </summary>
    [JsonProperty("format")]
    [YamlMember(Alias = "format")]
    public string? Format { get; set; }

    /// <summary>
    /// Custom validation rules to apply
    /// </summary>
    [JsonProperty("validation")]
    [YamlMember(Alias = "validation")]
    public PropertyValidation? Validation { get; set; }

    /// <summary>
    /// Custom description to override the original schema description
    /// </summary>
    [JsonProperty("description")]
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    /// <summary>
    /// Custom type mapping (e.g., "DateTime" instead of "string")
    /// </summary>
    [JsonProperty("type")]
    [YamlMember(Alias = "type")]
    public string? Type { get; set; }

    /// <summary>
    /// Custom default value
    /// </summary>
    [JsonProperty("default")]
    [YamlMember(Alias = "default")]
    public object? Default { get; set; }

    /// <summary>
    /// Gets whether this property should be included (defaulting to true if not specified)
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public bool IsIncluded => Include ?? true;
}

/// <summary>
/// Validation configuration for a property
/// </summary>
public class PropertyValidation
{
    /// <summary>
    /// Whether the property is required
    /// </summary>
    [JsonProperty("required")]
    [YamlMember(Alias = "required")]
    public bool? Required { get; set; }

    /// <summary>
    /// Minimum length for string properties
    /// </summary>
    [JsonProperty("minLength")]
    [YamlMember(Alias = "minLength")]
    public int? MinLength { get; set; }

    /// <summary>
    /// Maximum length for string properties
    /// </summary>
    [JsonProperty("maxLength")]
    [YamlMember(Alias = "maxLength")]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Regular expression pattern for validation
    /// </summary>
    [JsonProperty("pattern")]
    [YamlMember(Alias = "pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    /// Minimum value for numeric properties
    /// </summary>
    [JsonProperty("minimum")]
    [YamlMember(Alias = "minimum")]
    public decimal? Minimum { get; set; }

    /// <summary>
    /// Maximum value for numeric properties
    /// </summary>
    [JsonProperty("maximum")]
    [YamlMember(Alias = "maximum")]
    public decimal? Maximum { get; set; }

    /// <summary>
    /// Custom validation message
    /// </summary>
    [JsonProperty("message")]
    [YamlMember(Alias = "message")]
    public string? Message { get; set; }
}