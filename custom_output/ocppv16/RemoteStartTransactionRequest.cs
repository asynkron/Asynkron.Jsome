using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16RemoteStartTransactionRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("idTag")]
    [Required]
    [MaxLength(20)]
    public string IdTag { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("chargingProfile")]
    public object ChargingProfile { get; set; }

}