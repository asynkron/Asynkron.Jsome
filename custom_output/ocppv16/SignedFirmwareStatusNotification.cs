using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SignedFirmwareStatusNotification
{
    /// <summary>
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public V16FirmwareStatusEnumType Status { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("requestId")]
    public int RequestId { get; set; }

}