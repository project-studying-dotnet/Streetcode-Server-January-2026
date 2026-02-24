using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class TimelineItemUpdateDTOValidator : AbstractValidator<UpdateTimelineItemDTO>
    {
        public TimelineItemUpdateDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateTimelineItemDTO.Id)));

            RuleFor(x => x.Title)
                    .NotEmpty()
                    .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateTimelineItemDTO.Title)))
                    .MaximumLength(100)
                    .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(UpdateTimelineItemDTO.Title), 100));

            RuleFor(x => x.Description)
                .MaximumLength(600)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(Messages.Error_AdditionalTextMustNotExceedCharacters.Format(600));

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateTimelineItemDTO.Date)));

            RuleFor(x => x.DateViewPattern)
                .IsInEnum()
                .WithMessage(string.Format(Messages.Error_EntitiesNotFound, nameof(UpdateTimelineItemDTO.DateViewPattern)));

            RuleFor(x => x.StreetcodeId)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateTimelineItemDTO.StreetcodeId)));

            RuleForEach(x => x.HistoricalContextIds)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateTimelineItemDTO.HistoricalContextIds)));
        }
    }
}
