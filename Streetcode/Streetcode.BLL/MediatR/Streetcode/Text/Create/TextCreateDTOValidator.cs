using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create
{
    public class TextCreateDTOValidator : TextBaseValidator<TextCreateDTO>
    {
        public TextCreateDTOValidator()
        {
            BaseTextRules(
                x => x.Title,
                x => x.TextContent,
                x => x.AdditionalText,
                x => x.StreetcodeId);
        }
    }
}
