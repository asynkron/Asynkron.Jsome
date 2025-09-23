using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16SignCertificate
{
    /// <summary>
    /// </summary>
    [JsonProperty("csr")]
    [Required]
    [MaxLength(5500)]
    public string Csr { get; set; }

}