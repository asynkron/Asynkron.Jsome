using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16BootNotificationResponse
{
    /// <summary>
    /// Allowed values: Accepted, Pending, Rejected
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("currentTime")]
    [Required]
    public DateTime CurrentTime { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("interval")]
    [Required]
    public int Interval { get; set; }

}