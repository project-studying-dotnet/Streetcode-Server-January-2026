using FluentValidation;
using Streetcode.Shared.Extensions;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;

public class GetAllRelatedTermsByTermIdQueryValidator : AbstractValidator<GetAllRelatedTermsByTermIdQuery>
{
    public GetAllRelatedTermsByTermIdQueryValidator()
    {
        RuleFor(x => x.TermId)
            .GreaterThan(0)
            .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(GetAllRelatedTermsByTermIdQuery.TermId)));
    }
}