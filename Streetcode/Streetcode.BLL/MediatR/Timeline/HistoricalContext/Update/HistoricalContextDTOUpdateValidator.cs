using FluentValidation;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update
{
    public class HistoricalContextDTOUpdateValidator : AbstractValidator<UpdateHistoricalContextDTO>
    {
        public HistoricalContextDTOUpdateValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateHistoricalContextDTO.Id)));

            RuleFor(x => x.Title)
                    .NotEmpty()
                    .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateHistoricalContextDTO.Title)))
                    .MaximumLength(50)
                    .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(UpdateHistoricalContextDTO.Title), 50));
        }
    }
}
