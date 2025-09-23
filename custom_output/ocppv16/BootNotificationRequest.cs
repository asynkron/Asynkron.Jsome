using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16BootNotificationRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("chargePointVendor")]
    [Required]
    [MaxLength(20)]
    public string ChargePointVendor { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("chargePointModel")]
    [Required]
    [MaxLength(20)]
    public string ChargePointModel { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("chargePointSerialNumber")]
    [MaxLength(25)]
    public string ChargePointSerialNumber { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("chargeBoxSerialNumber")]
    [MaxLength(25)]
    public string ChargeBoxSerialNumber { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("firmwareVersion")]
    [MaxLength(50)]
    public string FirmwareVersion { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("iccid")]
    [MaxLength(20)]
    public string Iccid { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("imsi")]
    [MaxLength(20)]
    public string Imsi { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("meterType")]
    [MaxLength(25)]
    public string MeterType { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("meterSerialNumber")]
    [MaxLength(25)]
    public string MeterSerialNumber { get; set; }

}