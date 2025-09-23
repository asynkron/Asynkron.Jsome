using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16DataTransferResponse
{
    /// <summary>
    /// Allowed values: Accepted, Rejected, UnknownMessageId, UnknownVendorId
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }

}