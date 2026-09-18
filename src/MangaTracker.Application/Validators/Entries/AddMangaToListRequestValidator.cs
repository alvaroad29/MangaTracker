using FluentValidation;
using MangaTracker.Application.DTOs.Entries;

namespace MangaTracker.Application.Validators.Entries;

public class AddMangaToListRequestValidator : AbstractValidator<TrackMangaRequest>
{
    public AddMangaToListRequestValidator()
    {
        RuleFor(x => x.MalId).GreaterThan(0);
        RuleFor(x => x.Status).IsInEnum();
    }
}