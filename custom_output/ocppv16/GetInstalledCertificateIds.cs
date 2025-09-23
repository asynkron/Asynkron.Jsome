using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetInstalledCertificateIds
{
    /// <summary>
    /// </summary>
    [JsonProperty("certificateType")]
    [Required]
    public V16CertificateUseEnumType CertificateType { get; set; }

}