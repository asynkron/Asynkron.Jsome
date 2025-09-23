using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SendLocalListRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("listVersion")]
    [Required]
    public int ListVersion { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("localAuthorizationList")]
    public List<object> LocalAuthorizationList { get; set; }

    /// <summary>
    /// Allowed values: Differential, Full
    /// </summary>
    [JsonProperty("updateType")]
    [Required]
    public string UpdateType { get; set; }

}