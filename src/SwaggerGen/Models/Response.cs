using Newtonsoft.Json;

namespace SwaggerGen.Models;

/// <summary>
/// Describes a single response from an API Operation
/// </summary>
public class Response
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("schema")]
    public Schema? Schema { get; set; }

    [JsonProperty("headers")]
    public Dictionary<string, Header> Headers { get; set; } = new();

    [JsonProperty("examples")]
    public Dictionary<string, object> Examples { get; set; } = new();
}

/// <summary>
/// Describes a single header object
/// </summary>
public class Header
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("format")]
    public string Format { get; set; } = string.Empty;

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

/// <summary>
/// A limited subset of JSON-Schema's items object
/// </summary>
public class Items
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("format")]
    public string Format { get; set; } = string.Empty;

    [JsonProperty("items")]
    public Items? ItemsProperty { get; set; }

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