using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.AdminDelete
{
    public class AdminDeleteCommentCommandValidator : AbstractValidator<AdminDeleteCommentCommand>
    {
        public AdminDeleteCommentCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(AdminDeleteCommentCommand.Id)));
        }
    }
}
