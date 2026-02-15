using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public class UpdateFactDTOValidator : AbstractValidator<UpdateFactDTO>
    {
        public UpdateFactDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateFactDTO.Id)));

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateFactDTO.Title)))
                .MaximumLength(68).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(UpdateFactDTO.Title), 68));

            RuleFor(x => x.FactContent)
                .NotEmpty().WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateFactDTO.FactContent)))
                .MaximumLength(600).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(UpdateFactDTO.FactContent), 600));

            RuleFor(x => x.ImageId)
                .GreaterThan(0).WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateFactDTO.ImageId)));

            RuleFor(x => x.ImageDescription)
                .MaximumLength(200).WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(UpdateFactDTO.ImageDescription), 200));
        }
    }
}
