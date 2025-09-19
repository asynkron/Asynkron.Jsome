using FluentValidation;

namespace Generated.DTOs.Validators;

/// <summary>
/// FluentValidation validator for Pet
/// </summary>
public class PetValidator : AbstractValidator<Pet>
{
    public PetValidator()
    {
        RuleFor(x =&gt; x.Id).NotEmpty();
    }
}