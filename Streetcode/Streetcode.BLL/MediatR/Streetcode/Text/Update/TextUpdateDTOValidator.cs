using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public class TextUpdateDTOValidator : TextBaseValidator<TextUpdateDTO>
    {
        public TextUpdateDTOValidator()
        {
            BaseTextRules(
                x => x.Title,
                x => x.TextContent,
                x => x.AdditionalText,
                x => x.StreetcodeId);

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Text Id must be greater than zero");
        }
    }
}