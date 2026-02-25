using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;

public class UpdateRelatedTermDTOValidator : AbstractValidator<UpdateRelatedTermDTO>
{
    public UpdateRelatedTermDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateRelatedTermDTO.Id)));

        RuleFor(x => x.Word)
            .NotEmpty()
            .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateRelatedTermDTO.Word)))
            .MaximumLength(50)
            .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(UpdateRelatedTermDTO.Word), 50));
    }
}