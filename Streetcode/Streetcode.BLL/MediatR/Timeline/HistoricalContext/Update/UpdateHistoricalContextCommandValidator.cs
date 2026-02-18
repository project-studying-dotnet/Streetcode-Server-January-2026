using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update
{
    public class UpdateHistoricalContextCommandValidator : AbstractValidator<UpdateHistoricalContextCommand>
    {
        public UpdateHistoricalContextCommandValidator()
        {
            RuleFor(x => x.HistoricalContext)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new HistoricalContextDTOUpdateValidator());
        }
    }
}
