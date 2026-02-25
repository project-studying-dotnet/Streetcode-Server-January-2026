using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Create
{
    public class CreateCommentDTOValidator : AbstractValidator<CreateCommentDTO>
    {
        public CreateCommentDTOValidator()
        {
            RuleFor(x => x.TextContent)
                .NotEmpty().WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateCommentDTO.TextContent)))
                .MaximumLength(250).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                    nameof(CreateCommentDTO.TextContent),
                    250));

            RuleFor(x => x.StreetcodeId)
                .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreateCommentDTO.StreetcodeId)));
        }
    }
}
