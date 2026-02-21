using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Update
{
    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.Comment)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new UpdateCommentDTOValidator());

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format("UserId"));
        }
    }
}
