using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus
{
    public class UpdateCommentStatusCommandValidator : AbstractValidator<UpdateCommentStatusCommand>
    {
        public UpdateCommentStatusCommandValidator()
        {
            RuleFor(x => x.Comment)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new UpdateCommentStatusDTOValidator());
        }
    }
}
