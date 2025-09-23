using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SignCertificateResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public V16GenericStatusEnumType Status { get; set; }

}