using FluentValidation;

namespace Generated.DTOs.Validators;

/// <summary>
/// FluentValidation validator for NewPet
/// </summary>
public class NewPetValidator : AbstractValidator<NewPet>
{
    public NewPetValidator()
    {
        RuleFor(x =&gt; x.Name).NotEmpty();
    }
}