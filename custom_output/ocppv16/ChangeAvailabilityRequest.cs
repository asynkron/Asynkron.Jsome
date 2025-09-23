using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ChangeAvailabilityRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// Allowed values: Inoperative, Operative
    /// </summary>
    [JsonProperty("type")]
    [Required]
    public string Type { get; set; }

}