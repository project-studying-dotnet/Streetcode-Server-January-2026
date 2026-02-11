using FluentValidation;
using Streetcode.BLL.MediatR.Timeline.TimelineItem;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class UpdateTimelineItemDTOValidator : CreateTimelineItemDTOValidator<TimelineItemUpdateDTO>
    {
        public UpdateTimelineItemDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID must be greater than 0 for an update operation.");
        }
    }
}