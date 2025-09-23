using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetLogResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public V16LogStatusEnumType Status { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("filename")]
    [MaxLength(255)]
    public string Filename { get; set; }

}