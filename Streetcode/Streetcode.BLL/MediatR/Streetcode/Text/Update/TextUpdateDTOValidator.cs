using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public class TextUpdateDTOValidator : AbstractValidator<TextUpdateDTO>
    {
        public TextUpdateDTOValidator()
        {
            Include(new TextBaseValidator());

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(TextUpdateDTO.Id)));
        }
    }
}