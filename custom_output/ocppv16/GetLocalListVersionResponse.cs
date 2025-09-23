using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetLocalListVersionResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("listVersion")]
    [Required]
    public int ListVersion { get; set; }

}