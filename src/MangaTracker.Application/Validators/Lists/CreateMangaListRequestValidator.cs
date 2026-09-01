using FluentValidation;
using MangaTracker.Application.DTOs.Lists;

namespace MangaTracker.Application.Validators.Lists;

public class CreateMangaListRequestValidator : AbstractValidator<CreateMangaListRequest>
{
    public CreateMangaListRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}