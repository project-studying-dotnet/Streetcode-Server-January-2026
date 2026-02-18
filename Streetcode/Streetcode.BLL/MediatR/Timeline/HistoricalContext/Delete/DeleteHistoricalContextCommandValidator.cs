using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete
{
    public class DeleteHistoricalContextCommandValidator : AbstractValidator<DeleteHistoricalContextCommand>
    {
        public DeleteHistoricalContextCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(DeleteHistoricalContextCommand.Id)));
        }
    }
}
