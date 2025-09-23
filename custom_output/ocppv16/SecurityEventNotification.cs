using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SecurityEventNotification
{
    /// <summary>
    /// </summary>
    [JsonProperty("type")]
    [Required]
    [MaxLength(50)]
    public string Type { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("timestamp")]
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("techInfo")]
    [MaxLength(255)]
    public string TechInfo { get; set; }

}