using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.MediatR.Streetcode.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create
{
    public class TextCreateDTOValidator : AbstractValidator<TextCreateDTO>
    {
        public TextCreateDTOValidator()
        {
            Include(new TextBaseValidator());
        }
    }
}
