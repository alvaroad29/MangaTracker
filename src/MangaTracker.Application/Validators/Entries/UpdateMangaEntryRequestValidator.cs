using FluentValidation;
using MangaTracker.Application.DTOs.Entries;

namespace MangaTracker.Application.Validators.Entries;

public class UpdateMangaEntryRequestValidator : AbstractValidator<UpdateMangaEntryRequest>
{
    public UpdateMangaEntryRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.LastChapterRead).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Score).InclusiveBetween(1, 10).When(x => x.Score.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.FinishedAt)
            .GreaterThanOrEqualTo(x => x.StartedAt!.Value)
            .When(x => x.StartedAt.HasValue && x.FinishedAt.HasValue)
            .WithMessage("La fecha de finalización no puede ser anterior a la de inicio.");
    }
}