using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateTimelineItemCommandValidator : AbstractValidator<CreateHistoricalContextCommand>
    {
        public CreateTimelineItemCommandValidator()
        {
            RuleFor(x => x.TimelineItem)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new TimelineItemCreateDTOValidator());
        }
    }
}
