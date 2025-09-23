using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OCPP.V16.Generated;

/// <summary>
/// 
/// </summary>
public partial class V16FirmwareStatusNotificationRequest
{
    /// <summary>
    /// Allowed values: Downloaded, DownloadFailed, Downloading, Idle, InstallationFailed, Installing, Installed
    /// </summary>
    [JsonProperty("status")]
    [Required]
    public string Status { get; set; }

}