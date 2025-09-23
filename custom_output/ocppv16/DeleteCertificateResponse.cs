using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16DeleteCertificateResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public V16DeleteCertificateStatusEnumType Status { get; set; }

}