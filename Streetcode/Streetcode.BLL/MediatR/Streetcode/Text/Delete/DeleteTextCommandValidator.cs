using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Delete
{
    public class DeleteTextCommandValidator : AbstractValidator<DeleteTextCommand>
    {
        public DeleteTextCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Text Id must be greater than zero");
        }
    }
}
