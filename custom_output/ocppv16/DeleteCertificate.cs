using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16DeleteCertificate
{
    /// <summary>
    /// </summary>
    [JsonProperty("certificateHashData")]
    [Required]
    public V16CertificateHashDataType CertificateHashData { get; set; }

}