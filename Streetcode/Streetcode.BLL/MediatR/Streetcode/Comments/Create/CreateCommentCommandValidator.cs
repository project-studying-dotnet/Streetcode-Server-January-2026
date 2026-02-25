using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Create
{
    public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.Comment)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new CreateCommentDTOValidator());

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format("UserId"));
        }
    }
}
