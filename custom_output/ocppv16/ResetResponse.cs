using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ResetResponse
{
    /// <summary>
    /// Allowed values: Accepted, Rejected
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

}