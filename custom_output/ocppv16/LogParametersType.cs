using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16LogParametersType
{
    /// <summary>
    /// </summary>
    [JsonProperty("remoteLocation")]
    [Required]
    [MaxLength(512)]
    public string RemoteLocation { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("oldestTimestamp")]
    public DateTime OldestTimestamp { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("latestTimestamp")]
    public DateTime LatestTimestamp { get; set; }

}