using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ClearChargingProfileRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    public int ConnectorId { get; set; }

    /// <summary>
    /// Allowed values: ChargePointMaxProfile, TxDefaultProfile, TxProfile
    /// </summary>
    [JsonProperty("chargingProfilePurpose")]
    public string ChargingProfilePurpose { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("stackLevel")]
    public int StackLevel { get; set; }

}