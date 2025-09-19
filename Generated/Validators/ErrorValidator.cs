using FluentValidation;

public class ErrorValidator : AbstractValidator<Error>
{
    public ErrorValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Message).NotEmpty();
    }
}