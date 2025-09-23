using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16HeartbeatResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("currentTime")]
    [Required]
    public DateTime CurrentTime { get; set; }

}