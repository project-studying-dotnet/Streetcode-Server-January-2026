using FluentValidation;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;

public class DeleteRelatedTermCommandValidator : AbstractValidator<DeleteRelatedTermCommand>
{
    public DeleteRelatedTermCommandValidator()
    {
        RuleFor(x => x.TermId)
            .GreaterThan(0)
            .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(DeleteRelatedTermCommand.TermId)));

        RuleFor(x => x.Word)
            .NotEmpty()
            .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(DeleteRelatedTermCommand.Word)))
            .MaximumLength(50)
            .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(DeleteRelatedTermCommand.Word), 50));
    }
}