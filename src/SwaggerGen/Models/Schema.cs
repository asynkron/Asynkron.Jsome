using Newtonsoft.Json;

namespace SwaggerGen.Models;

/// <summary>
/// Allows the definition of input and output data types
/// </summary>
public class Schema
{
    [JsonProperty("$ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonProperty("format")]
    public string Format { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("default")]
    public object? Default { get; set; }

    [JsonProperty("multipleOf")]
    public decimal? MultipleOf { get; set; }

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

    [JsonProperty("maxProperties")]
    public int? MaxProperties { get; set; }

    [JsonProperty("minProperties")]
    public int? MinProperties { get; set; }

    [JsonProperty("required")]
    public List<string> Required { get; set; } = new();

    [JsonProperty("enum")]
    public List<object> Enum { get; set; } = new();

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("items")]
    public Schema? Items { get; set; }

    [JsonProperty("allOf")]
    public List<Schema> AllOf { get; set; } = new();

    [JsonProperty("properties")]
    public Dictionary<string, Schema> Properties { get; set; } = new();

    [JsonProperty("additionalProperties")]
    public Schema? AdditionalProperties { get; set; }

    [JsonProperty("discriminator")]
    public string Discriminator { get; set; } = string.Empty;

    [JsonProperty("readOnly")]
    public bool? ReadOnly { get; set; }

    [JsonProperty("xml")]
    public Xml? Xml { get; set; }

    [JsonProperty("externalDocs")]
    public ExternalDocs? ExternalDocs { get; set; }

    [JsonProperty("example")]
    public object? Example { get; set; }
}

/// <summary>
/// A metadata object that allows for more fine-tuned XML model definitions
/// </summary>
public class Xml
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("namespace")]
    public string Namespace { get; set; } = string.Empty;

    [JsonProperty("prefix")]
    public string Prefix { get; set; } = string.Empty;

    [JsonProperty("attribute")]
    public bool? Attribute { get; set; }

    [JsonProperty("wrapped")]
    public bool? Wrapped { get; set; }
}

/// <summary>
/// Allows referencing an external resource for extended documentation
/// </summary>
public class ExternalDocs
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
}