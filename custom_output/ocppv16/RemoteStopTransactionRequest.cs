using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16RemoteStopTransactionRequest
{
    /// <summary>
    /// </summary>
    [JsonProperty("transactionId")]
    [Required]
    public int TransactionId { get; set; }

}