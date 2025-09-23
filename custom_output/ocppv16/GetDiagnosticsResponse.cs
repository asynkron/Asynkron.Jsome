using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16GetDiagnosticsResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("fileName")]
    [MaxLength(255)]
    public string FileName { get; set; }

}