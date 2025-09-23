using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16StartTransactionResponse
{
    /// <summary>
    /// </summary>
    [JsonProperty("idTagInfo")]
    [Required]
    public object IdTagInfo { get; set; }

    /// <summary>
    /// </summary>
    [JsonProperty("transactionId")]
    [Required]
    public int TransactionId { get; set; }

}