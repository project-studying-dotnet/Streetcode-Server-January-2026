using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactDTOValidator : AbstractValidator<CreateFactDTO>
    {
        public CreateFactDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateFactDTO.Title)))
                .MaximumLength(68).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                    nameof(CreateFactDTO.Title),
                    68));

            RuleFor(x => x.FactContent)
                .NotEmpty().WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateFactDTO.FactContent)))
                .MaximumLength(600).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                    nameof(CreateFactDTO.FactContent),
                    600));

            RuleFor(x => x.ImageId)
                .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreateFactDTO.ImageId)));

            RuleFor(x => x.StreetcodeId)
                .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreateFactDTO.StreetcodeId)));

            RuleFor(x => x.ImageDescription)
                .MaximumLength(200).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                    nameof(CreateFactDTO.ImageDescription),
                    200));
        }
    }
}
