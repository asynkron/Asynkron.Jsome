using FluentValidation;

public class PetValidator : AbstractValidator<Pet>
{
    public PetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}