using FluentValidation;

public class NewPetValidator : AbstractValidator<NewPet>
{
    public NewPetValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}