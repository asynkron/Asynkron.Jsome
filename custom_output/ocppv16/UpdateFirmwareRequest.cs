using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16UpdateFirmwareRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("location")]
    [Required]
    public string Location { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retries")]
    public int Retries { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retrieveDate")]
    [Required]
    public DateTime RetrieveDate { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("retryInterval")]
    public int RetryInterval { get; set; }

}