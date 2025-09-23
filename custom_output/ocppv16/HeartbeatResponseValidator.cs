using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16HeartbeatResponse
/// </summary>
public class V16HeartbeatResponseValidator : AbstractValidator<V16HeartbeatResponse>
{
    public V16HeartbeatResponseValidator()
    {
        RuleFor(x => x.CurrentTime)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}