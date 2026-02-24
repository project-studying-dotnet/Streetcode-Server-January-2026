using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create
{
    public class CreateHistoricalContextCommandValidator : AbstractValidator<CreateHistoricalContextCommand>
    {
        public CreateHistoricalContextCommandValidator()
        {
            RuleFor(x => x.HistoricalContext)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new HistoricalContextCreateDTOValidator());
        }
    }
}
