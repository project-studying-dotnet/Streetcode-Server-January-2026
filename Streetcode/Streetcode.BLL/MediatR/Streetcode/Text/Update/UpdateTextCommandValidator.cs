using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public class UpdateTextCommandValidator : AbstractValidator<UpdateTextCommand>
    {
        public UpdateTextCommandValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new TextUpdateDTOValidator());
        }
    }
}
