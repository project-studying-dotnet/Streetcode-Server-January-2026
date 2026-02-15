using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public class UpdateOrderFactDTOListValidator : AbstractValidator<List<UpdateFactOrderDTO>>
    {
        public UpdateOrderFactDTOListValidator()
        {
            RuleFor(x => x)
                .NotEmpty().WithMessage(Messages.Error_FactsListEmpty);

            RuleForEach(x => x).ChildRules(fact =>
            {
                fact.RuleFor(item => item.Id)
                    .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateFactDTO.Id)));

                fact.RuleFor(item => item.Order)
                    .GreaterThanOrEqualTo(0).WithMessage(Messages.Error_PropertyMustBeEqualOrGreaterThanZero.Format(
                        nameof(UpdateFactOrderDTO.Order)));
            });
        }
    }
}
