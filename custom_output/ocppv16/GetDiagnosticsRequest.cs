using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetDiagnosticsRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("location")]
    [Required]
    public string Location { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retries")]
    public int Retries { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retryInterval")]
    public int RetryInterval { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("stopTime")]
    public DateTime StopTime { get; set; }

}