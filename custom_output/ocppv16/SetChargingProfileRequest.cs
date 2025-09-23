using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SetChargingProfileRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("csChargingProfiles")]
    [Required]
    public object CsChargingProfiles { get; set; }

}