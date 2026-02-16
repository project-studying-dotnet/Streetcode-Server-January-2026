using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create
{
    public class CreateTextCommandValidator : AbstractValidator<CreateTextCommand>
    {
        public CreateTextCommandValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new TextCreateDTOValidator());
        }
    }
}
