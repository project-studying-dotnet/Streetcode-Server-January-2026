using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Text.Update;
using Streetcode.BLL.MediatR.Streetcode.Text.Validator;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public class UpdateTextCommandValidator : AbstractValidator<UpdateTextCommand>
    {
        public UpdateTextCommandValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .WithMessage("TextDataRequired")
                .SetValidator(new TextBaseDTOValidator());
        }
    }
}
