using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ReserveNowRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("expiryDate")]
    [Required]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("idTag")]
    [Required]
    [MaxLength(20)]
    public string IdTag { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("parentIdTag")]
    [MaxLength(20)]
    public string ParentIdTag { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("reservationId")]
    [Required]
    public int ReservationId { get; set; }

}