using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SignedUpdateFirmwareResponse
/// </summary>
public class V16SignedUpdateFirmwareResponseValidator : AbstractValidator<V16SignedUpdateFirmwareResponse>
{
    public V16SignedUpdateFirmwareResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}