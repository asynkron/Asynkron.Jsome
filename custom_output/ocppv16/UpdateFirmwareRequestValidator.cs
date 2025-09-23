using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16UpdateFirmwareRequest
/// </summary>
public class V16UpdateFirmwareRequestValidator : AbstractValidator<V16UpdateFirmwareRequest>
{
    public V16UpdateFirmwareRequestValidator()
    {
        RuleFor(x => x.Location)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.RetrieveDate)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}