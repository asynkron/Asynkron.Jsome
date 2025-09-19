using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Generated.DTOs;

/// <summary>
/// DTO class for Pet
/// </summary>
public class Pet
{
    /// <summary>
    /// Gets or sets the id
    /// </summary>
    [Required]
    public long Id { get; set; }

}