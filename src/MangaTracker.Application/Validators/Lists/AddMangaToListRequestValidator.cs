using FluentValidation;
using MangaTracker.Application.DTOs.Lists;

namespace MangaTracker.Application.Validators.Lists;

public class AddMangaToListRequestValidator : AbstractValidator<AddMangaToListRequest>
{
    public AddMangaToListRequestValidator()
    {
        RuleFor(x => x.MalId).GreaterThan(0);
        RuleFor(x => x.DefaultStatus).IsInEnum();
    }
}