using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16ExtendedTriggerMessageResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public V16TriggerMessageStatusEnumType Status { get; set; }

}