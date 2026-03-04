using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus
{
    public class UpdateCommentStatusDTOValidator : AbstractValidator<UpdateCommentStatusDTO>
    {
        public UpdateCommentStatusDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateCommentStatusDTO.Id)));

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage(Messages.Error_InvalidEnumValue.Format(nameof(UpdateCommentStatusDTO.Status)));
        }
    }
}
