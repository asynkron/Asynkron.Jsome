using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ChangeAvailabilityRequest
/// </summary>
public class V16ChangeAvailabilityRequestValidator : AbstractValidator<V16ChangeAvailabilityRequest>
{
    public V16ChangeAvailabilityRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Type)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Inoperative", "Operative" }.Contains(x.ToString())).WithMessage("Must be one of: "Inoperative", "Operative"")
            ;
    }
}