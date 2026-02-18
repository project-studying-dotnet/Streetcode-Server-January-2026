using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class UpdateTimelineItemCommandValidator : AbstractValidator<UpdateTimelineItemCommand>
    {
        public UpdateTimelineItemCommandValidator()
        {
            RuleFor(x => x.TimelineItem)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new TimelineItemUpdateDTOValidator());
        }
    }
}
