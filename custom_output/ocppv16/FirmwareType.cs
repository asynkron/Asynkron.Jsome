using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16FirmwareType
{
    /// <summary>
    /// </summary>
    [JsonProperty("location")]
    [Required]
    [MaxLength(512)]
    public string Location { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retrieveDateTime")]
    [Required]
    public DateTime RetrieveDateTime { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("installDateTime")]
    public DateTime InstallDateTime { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("signingCertificate")]
    [Required]
    [MaxLength(5500)]
    public string SigningCertificate { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("signature")]
    [Required]
    [MaxLength(800)]
    public string Signature { get; set; }

}