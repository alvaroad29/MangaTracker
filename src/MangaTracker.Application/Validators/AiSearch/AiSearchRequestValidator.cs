using FluentValidation;
using MangaTracker.Application.DTOs.AiSearch;

namespace MangaTracker.Application.Validators.AiSearch;

public class AiSearchRequestValidator : AbstractValidator<AiSearchRequest>
{
    public AiSearchRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(300).WithMessage("Describí lo que buscás en menos de 300 caracteres.");
    }
}