using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16StatusNotificationRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// Allowed values: ConnectorLockFailure, EVCommunicationError, GroundFailure, HighTemperature, InternalError, LocalListConflict, NoError, OtherError, OverCurrentFailure, PowerMeterFailure, PowerSwitchFailure, ReaderFailure, ResetFailure, UnderVoltage, OverVoltage, WeakSignal
    /// </summary>
    [JsonProperty("errorCode")]
    [Required]
    public string ErrorCode { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("info")]
    [MaxLength(50)]
    public string Info { get; set; }

    /// <summary>
    /// Allowed values: Available, Preparing, Charging, SuspendedEVSE, SuspendedEV, Finishing, Reserved, Unavailable, Faulted
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("vendorId")]
    [MaxLength(255)]
    public string VendorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("vendorErrorCode")]
    [MaxLength(50)]
    public string VendorErrorCode { get; set; }

}