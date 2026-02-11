using FluentValidation;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

using Streetcode.BLL.Validators.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateTimelineItemDTOValidator : AbstractValidator<TimelineItemCreateDTO>
    {
        public CreateTimelineItemDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(28).WithMessage("Title length must not exceed 28 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(400).WithMessage("Description length must not exceed 400 characters.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Date cannot be in the future.");

            RuleFor(x => x.DateViewPattern)
                .IsInEnum().WithMessage("Provided date view pattern is not a valid value.");

            RuleForEach(x => x.HistoricalContexts)
                .NotNull().WithMessage("Historical context cannot be null.")
                .SetValidator(new HistoricalContextCreateDTOValidator());
        }
    }
}