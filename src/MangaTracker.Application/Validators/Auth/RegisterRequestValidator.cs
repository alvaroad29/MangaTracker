using FluentValidation;
using MangaTracker.Application.DTOs.Auth;

namespace MangaTracker.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe tener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");
        
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
    }
}