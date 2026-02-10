using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public class UpdateOrderFactDTOListValidator : AbstractValidator<List<UpdateFactOrderDTO>>
    {
        public UpdateOrderFactDTOListValidator()
        {
            RuleFor(x => x)
                .NotEmpty().WithMessage("Facts list cannot be empty.");

            RuleForEach(x => x).ChildRules(fact =>
            {
                fact.RuleFor(item => item.Id)
                    .GreaterThan(0).WithMessage("Fact Id must be greater than 0.");

                fact.RuleFor(item => item.Order)
                    .GreaterThanOrEqualTo(0).WithMessage("Order must be 0 or greater.");
            });
        }
    }
}
