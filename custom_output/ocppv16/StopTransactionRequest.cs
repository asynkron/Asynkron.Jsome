using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16StopTransactionRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("idTag")]
    [MaxLength(20)]
    public string IdTag { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("meterStop")]
    [Required]
    public int MeterStop { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("timestamp")]
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("transactionId")]
    [Required]
    public int TransactionId { get; set; }

    /// <summary>
    /// Allowed values: EmergencyStop, EVDisconnected, HardReset, Local, Other, PowerLoss, Reboot, Remote, SoftReset, UnlockCommand, DeAuthorized
    /// </summary>
    [JsonProperty("reason")]
    public string Reason { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("transactionData")]
    public List<object> TransactionData { get; set; }

}