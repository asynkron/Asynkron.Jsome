using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetInstalledCertificateIdsResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("certificateHashData")]
    public List<V16CertificateHashDataType> CertificateHashData { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public V16GetInstalledCertificateStatusEnumType Status { get; set; }

}