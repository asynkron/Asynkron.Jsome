using Newtonsoft.Json;

namespace SwaggerGen.Models;

/// <summary>
/// Represents a Swagger 2.0 document
/// </summary>
public class SwaggerDocument
{
    [JsonProperty("swagger")]
    public string Swagger { get; set; } = string.Empty;

    [JsonProperty("info")]
    public Info Info { get; set; } = new();

    [JsonProperty("host")]
    public string Host { get; set; } = string.Empty;

    [JsonProperty("basePath")]
    public string BasePath { get; set; } = string.Empty;

    [JsonProperty("schemes")]
    public List<string> Schemes { get; set; } = [];

    [JsonProperty("consumes")]
    public List<string> Consumes { get; set; } = [];

    [JsonProperty("produces")]
    public List<string> Produces { get; set; } = [];

    [JsonProperty("paths")]
    public Dictionary<string, PathItem> Paths { get; set; } = new();

    [JsonProperty("definitions")]
    public Dictionary<string, Schema> Definitions { get; set; } = new();

    [JsonProperty("parameters")]
    public Dictionary<string, Parameter> Parameters { get; set; } = new();

    [JsonProperty("responses")]
    public Dictionary<string, Response> Responses { get; set; } = new();
}