using FluentValidation;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create
{
    public class HistoricalContextCreateDTOValidator : AbstractValidator<CreateHistoricalContextDTO>
    {
        public HistoricalContextCreateDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateHistoricalContextDTO.Title)))
                .MaximumLength(50)
                .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(CreateHistoricalContextDTO.Title), 50));
        }
    }
}
