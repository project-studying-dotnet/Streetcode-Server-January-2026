using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public class TextUpdateDTOValidator : AbstractValidator<TextUpdateDTO>
    {
        public TextUpdateDTOValidator()
        {
            Include(new TextBaseValidator());

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Text TermId must be greater than zero");
        }
    }
}