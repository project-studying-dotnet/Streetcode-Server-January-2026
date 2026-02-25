using FluentValidation;
using Streetcode.Email.BLL.DTO;

namespace Streetcode.Email.BLL.MediatR.Feedback
{
    public class FeedbackDTOValidator : AbstractValidator<FeedbackDTO>
    {
        public FeedbackDTOValidator()
        { 
        RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

        RuleFor(x => x.Message)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(100);
        }
    }
}
