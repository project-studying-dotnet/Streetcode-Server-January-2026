using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Update
{
    public class UpdateCommentDTOValidator : AbstractValidator<UpdateCommentDTO>
    {
        public UpdateCommentDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateCommentDTO.Id)));

            RuleFor(x => x.TextContent)
                .NotEmpty().WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateCommentDTO.TextContent)))
                .MaximumLength(250).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                    nameof(UpdateCommentDTO.TextContent),
                    250));
        }
    }
}
