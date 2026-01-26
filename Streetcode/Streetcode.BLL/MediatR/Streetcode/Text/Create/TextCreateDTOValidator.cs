using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Create
{
    public class TextCreateDTOValidator : AbstractValidator<TextCreateDTO>
    {
        public TextCreateDTOValidator()
        {
            RuleFor(x => x.StreetcodeId)
                .GreaterThan(0)
                .WithMessage("StreetcodeId must be greater than zero");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(300)
                .WithMessage("Title must not exceed 300 characters");

            RuleFor(x => x.TextContent)
                .NotEmpty()
                .WithMessage("TextContent is required")
                .MaximumLength(1500)
                .WithMessage("TextContent must not exceed 1500 characters");

            RuleFor(x => x.AdditionalText)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.AdditionalText))
                .WithMessage("If AdditionalText is provided, it must not exceed 500 characters");
        }
    }
}
