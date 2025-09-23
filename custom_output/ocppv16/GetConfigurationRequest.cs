using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetConfigurationRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("key")]
    public List<string> Key { get; set; }

}