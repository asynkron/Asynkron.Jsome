using Newtonsoft.Json;

namespace Asynkron.Jsome.Models;

/// <summary>
/// Provides metadata about the API
/// </summary>
public class Info
{
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("termsOfService")]
    public string TermsOfService { get; set; } = string.Empty;

    [JsonProperty("contact")]
    public Contact? Contact { get; set; }

    [JsonProperty("license")]
    public License? License { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Contact information for the exposed API
/// </summary>
public class Contact
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// License information for the exposed API
/// </summary>
public class License
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
}