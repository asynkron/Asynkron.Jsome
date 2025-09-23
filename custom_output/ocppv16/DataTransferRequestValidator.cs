using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16DataTransferRequest
/// </summary>
public class V16DataTransferRequestValidator : AbstractValidator<V16DataTransferRequest>
{
    public V16DataTransferRequestValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(255).WithMessage("Must be no more than 255 characters long")
            ;
        RuleFor(x => x.MessageId)
            .MaximumLength(50).WithMessage("Must be no more than 50 characters long")
            ;
    }
}