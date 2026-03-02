using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;

public class CreateRelatedTermDTOValidator : AbstractValidator<CreateRelatedTermDTO>
{
    public CreateRelatedTermDTOValidator()
    {
        RuleFor(x => x.TermId)
            .GreaterThan(0)
            .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreateRelatedTermDTO.TermId)));

        RuleFor(x => x.Word)
            .NotEmpty()
            .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateRelatedTermDTO.Word)))
            .MaximumLength(50)
            .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(CreateRelatedTermDTO.Word), 50));
    }
}