using System.ComponentModel.DataAnnotations;
public class Pet : NewPet
{
    [Required]
    public long Id { get; set; }

}