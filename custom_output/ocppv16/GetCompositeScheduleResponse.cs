using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetCompositeScheduleResponse
{
    /// <summary>
    /// Allowed values: Accepted, Rejected
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("scheduleStart")]
    public DateTime ScheduleStart { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("chargingSchedule")]
    public object ChargingSchedule { get; set; }

}