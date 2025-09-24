using Newtonsoft.Json;

namespace Asynkron.Jsome.Models;

/// <summary>
/// Describes the operations available on a single path
/// </summary>
public class PathItem
{
    [JsonProperty("get")]
    public Operation? Get { get; set; }

    [JsonProperty("put")]
    public Operation? Put { get; set; }

    [JsonProperty("post")]
    public Operation? Post { get; set; }

    [JsonProperty("delete")]
    public Operation? Delete { get; set; }

    [JsonProperty("options")]
    public Operation? Options { get; set; }

    [JsonProperty("head")]
    public Operation? Head { get; set; }

    [JsonProperty("patch")]
    public Operation? Patch { get; set; }

    [JsonProperty("parameters")]
    public List<Parameter> Parameters { get; set; } = [];
}

/// <summary>
/// Describes a single API operation on a path
/// </summary>
public class Operation
{
    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("operationId")]
    public string OperationId { get; set; } = string.Empty;

    [JsonProperty("consumes")]
    public List<string> Consumes { get; set; } = [];

    [JsonProperty("produces")]
    public List<string> Produces { get; set; } = [];

    [JsonProperty("parameters")]
    public List<Parameter> Parameters { get; set; } = [];

    [JsonProperty("responses")]
    public Dictionary<string, Response> Responses { get; set; } = new();

    [JsonProperty("schemes")]
    public List<string> Schemes { get; set; } = [];

    [JsonProperty("deprecated")]
    public bool Deprecated { get; set; }
}