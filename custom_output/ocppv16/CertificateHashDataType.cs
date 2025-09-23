using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16CertificateHashDataType
{
    /// <summary>
    /// </summary>
    [JsonProperty("hashAlgorithm")]
    [Required]
    public V16HashAlgorithmEnumType HashAlgorithm { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("issuerNameHash")]
    [Required]
    [MaxLength(128)]
    public string IssuerNameHash { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("issuerKeyHash")]
    [Required]
    [MaxLength(128)]
    public string IssuerKeyHash { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("serialNumber")]
    [Required]
    [MaxLength(40)]
    public string SerialNumber { get; set; }

}