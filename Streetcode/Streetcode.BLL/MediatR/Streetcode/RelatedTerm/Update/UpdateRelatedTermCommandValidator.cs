using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;

public class UpdateRelatedTermCommandValidator : AbstractValidator<UpdateRelatedTermCommand>
{
    public UpdateRelatedTermCommandValidator()
    {
        RuleFor(x => x.UpdateRelatedTerm)
            .NotNull()
            .WithMessage(Messages.Error_CommandDataRequired)
            .SetValidator(new UpdateRelatedTermDTOValidator());
    }
}