using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public class DeleteTimelineItemCommandValidator : AbstractValidator<DeleteTimelineItemCommand>
    {
        public DeleteTimelineItemCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(DeleteTimelineItemCommand.Id)));
        }
    }
}
