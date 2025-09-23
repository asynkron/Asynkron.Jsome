using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ChangeConfigurationRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("key")]
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    public string Key { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("value")]
    [Required]
    [MaxLength(500)]
    [MinLength(1)]
    public string Value { get; set; }

}