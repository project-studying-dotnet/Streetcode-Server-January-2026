using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Delete
{
    public class DeleteTextCommandValidator : AbstractValidator<DeleteTextCommand>
    {
        public DeleteTextCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(DeleteTextCommand.Id)));
        }
    }
}
