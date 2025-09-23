using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16DataTransferRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("vendorId")]
    [Required]
    [MaxLength(255)]
    public string VendorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("messageId")]
    [MaxLength(50)]
    public string MessageId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }

}