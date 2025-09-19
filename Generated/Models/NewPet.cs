using System.ComponentModel.DataAnnotations;
public class NewPet
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Tag { get; set; }

}