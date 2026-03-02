using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;

public class CreateRelatedTermCommandValidator : AbstractValidator<CreateRelatedTermCommand>
{
    public CreateRelatedTermCommandValidator()
    {
        RuleFor(x => x.CreateRelatedTerm)
            .NotNull()
            .WithMessage(Messages.Error_CommandDataRequired)
            .SetValidator(new CreateRelatedTermDTOValidator());
    }
}