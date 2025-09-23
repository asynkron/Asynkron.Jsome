using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetConfigurationResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("configurationKey")]
    public List<object> ConfigurationKey { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("unknownKey")]
    public List<string> UnknownKey { get; set; }

}