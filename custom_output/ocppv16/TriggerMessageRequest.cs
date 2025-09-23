using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16TriggerMessageRequest
{
    /// <summary>
    /// Allowed values: BootNotification, DiagnosticsStatusNotification, FirmwareStatusNotification, Heartbeat, MeterValues, StatusNotification
    /// </summary>
    [JsonProperty("requestedMessage")]
    [Required]
    public string RequestedMessage { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }

}