using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16StartTransactionRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("idTag")]
    [Required]
    [MaxLength(20)]
    public string IdTag { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("meterStart")]
    [Required]
    public int MeterStart { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("reservationId")]
    public int ReservationId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("timestamp")]
    [Required]
    public DateTime Timestamp { get; set; }

}