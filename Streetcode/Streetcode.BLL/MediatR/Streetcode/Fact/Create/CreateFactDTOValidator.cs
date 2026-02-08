using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactDTOValidator : AbstractValidator<CreateFactDTO>
    {
        public CreateFactDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(68).WithMessage("Title length must not exceed 68 characters.");

            RuleFor(x => x.FactContent)
                .NotEmpty().WithMessage("Fact content is required.")
                .MaximumLength(600).WithMessage("Fact content length must not exceed 600 characters.");

            RuleFor(x => x.ImageId)
                .GreaterThan(0).WithMessage("ImageId must be greater than 0.");

            RuleFor(x => x.StreetcodeId)
                .GreaterThan(0).WithMessage("StreetcodeId must be greater than 0.");

            RuleFor(x => x.ImageDescription)
                .MaximumLength(200).WithMessage("Image description must not exceed 200 characters.");
        }
    }
}
