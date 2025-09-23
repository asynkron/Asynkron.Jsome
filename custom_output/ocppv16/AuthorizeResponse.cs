using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16AuthorizeResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("idTagInfo")]
    [Required]
    public object IdTagInfo { get; set; }

}