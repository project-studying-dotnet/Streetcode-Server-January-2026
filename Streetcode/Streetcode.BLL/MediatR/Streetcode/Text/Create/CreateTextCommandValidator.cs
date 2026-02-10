using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create
{
    public class CreateTextCommandValidator : AbstractValidator<CreateTextCommand>
    {
        public CreateTextCommandValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .WithMessage("TextDataRequired")
                .SetValidator(new TextCreateDTOValidator());
        }
    }
}
