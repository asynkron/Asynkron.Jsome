using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Generated.DTOs;

/// <summary>
/// DTO class for NewPet
/// </summary>
public class NewPet
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the tag
    /// </summary>
    public string? Tag { get; set; }

}