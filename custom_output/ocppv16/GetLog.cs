using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetLog
{
    /// <summary>
    /// </summary>
    [JsonProperty("log")]
    [Required]
    public V16LogParametersType Log { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("logType")]
    [Required]
    public V16LogEnumType LogType { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("requestId")]
    [Required]
    public int RequestId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retries")]
    public int Retries { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retryInterval")]
    public int RetryInterval { get; set; }

}