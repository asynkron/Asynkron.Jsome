using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ClearChargingProfileResponse
{
    /// <summary>
    /// Allowed values: Accepted, Unknown
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

}