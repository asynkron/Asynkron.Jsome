using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SignedUpdateFirmware
{
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
    [JsonProperty("requestId")]
    [Required]
    public int RequestId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("firmware")]
    [Required]
    public V16FirmwareType Firmware { get; set; }

}