using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16MeterValuesRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("connectorId")]
    [Required]
    public int ConnectorId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("transactionId")]
    public int TransactionId { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("meterValue")]
    [Required]
    public List<object> MeterValue { get; set; }

}