using System.Linq.Expressions;
using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Text
{
    public class TextBaseValidator<T> : AbstractValidator<T>
    {
        protected void BaseTextRules(
            Expression<Func<T, string>> Title,
            Expression<Func<T, string>> TextContent,
            Expression<Func<T, string?>> AdditionalText,
            Expression<Func<T, int>> StreetcodeId)
        {
            RuleFor(StreetcodeId)
                .GreaterThan(0)
                .WithMessage("StreetcodeId must be greater than zero");

            RuleFor(Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(300)
                .WithMessage("Title must not exceed 300 characters");

            RuleFor(TextContent)
                .NotEmpty()
                .WithMessage("TextContent is required")
                .MaximumLength(1500)
                .WithMessage("TextContent must not exceed 1500 characters");

            RuleFor(AdditionalText)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(AdditionalText.Compile()(x)))
                .WithMessage("If AdditionalText is provided, it must not exceed 500 characters");
        }
    }
}
