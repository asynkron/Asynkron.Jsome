using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ResetRequest
{
    /// <summary>
    /// Allowed values: Hard, Soft
    /// </summary>
    [JsonProperty("type")]
    [Required]
    public string Type { get; set; }

}