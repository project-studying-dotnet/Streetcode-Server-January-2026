using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateHistoricalContextCommandValidator : AbstractValidator<CreateHistoricalContextCommand>
    {
        public CreateHistoricalContextCommandValidator()
        {
            RuleFor(x => x.TimelineItem)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new TimelineItemCreateDTOValidator());
        }
    }
}
