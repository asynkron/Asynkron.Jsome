using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Generated.DTOs;

/// <summary>
/// DTO class for Error
/// </summary>
public class Error
{
    /// <summary>
    /// Gets or sets the code
    /// </summary>
    [Required]
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    public string Message { get; set; }

}