using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Delete
{
    public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(DeleteCommentCommand.Id)));

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format("UserId"));
        }
    }
}
