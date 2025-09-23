using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetCompositeScheduleRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("duration")]
    [Required]
    public int Duration { get; set; }

    /// <summary>
    /// Allowed values: A, W
    /// </summary>
    [JsonProperty("chargingRateUnit")]
    public string ChargingRateUnit { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("transactionId")]
    public int TransactionId { get; set; }

}