using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16CancelReservationRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("reservationId")]
    [Required]
    public int ReservationId { get; set; }

}