using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Validator
{
    public class TextBaseDTOValidator : AbstractValidator<TextBaseDTO>
    {
        public TextBaseDTOValidator()
        {
            RuleFor(x => x.StreetcodeId)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(TextBaseDTO.StreetcodeId)));

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(TextBaseDTO.Title)))
                .MaximumLength(300)
                .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(TextBaseDTO.Title), 300));

            RuleFor(x => x.TextContent)
                .NotEmpty()
                .WithMessage(Messages.Error_PropertyIsRequired.Format(nameof(TextBaseDTO.TextContent)))
                .MaximumLength(1500)
                .WithMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(TextBaseDTO.TextContent), 1500));

            RuleFor(x => x.AdditionalText)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.AdditionalText))
                .WithMessage(Messages.Error_AdditionalTextMustNotExceedCharacters.Format(500));
        }
    }
}
