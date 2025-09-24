using Newtonsoft.Json;

namespace Asynkron.Jsome.Models;

/// <summary>
/// Describes a single operation parameter
/// </summary>
public class Parameter
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("in")]
    public string In { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("required")]
    public bool Required { get; set; }

    [JsonProperty("schema")]
    public Schema? Schema { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("format")]
    public string Format { get; set; } = string.Empty;

    [JsonProperty("allowEmptyValue")]
    public bool AllowEmptyValue { get; set; }

    [JsonProperty("items")]
    public Items? Items { get; set; }

    [JsonProperty("collectionFormat")]
    public string CollectionFormat { get; set; } = string.Empty;

    [JsonProperty("default")]
    public object? Default { get; set; }

    [JsonProperty("maximum")]
    public decimal? Maximum { get; set; }

    [JsonProperty("exclusiveMaximum")]
    public bool? ExclusiveMaximum { get; set; }

    [JsonProperty("minimum")]
    public decimal? Minimum { get; set; }

    [JsonProperty("exclusiveMinimum")]
    public bool? ExclusiveMinimum { get; set; }

    [JsonProperty("maxLength")]
    public int? MaxLength { get; set; }

    [JsonProperty("minLength")]
    public int? MinLength { get; set; }

    [JsonProperty("pattern")]
    public string Pattern { get; set; } = string.Empty;

    [JsonProperty("maxItems")]
    public int? MaxItems { get; set; }

    [JsonProperty("minItems")]
    public int? MinItems { get; set; }

    [JsonProperty("uniqueItems")]
    public bool? UniqueItems { get; set; }

    [JsonProperty("enum")]
    public List<object> Enum { get; set; } = [];

    [JsonProperty("multipleOf")]
    public decimal? MultipleOf { get; set; }
}