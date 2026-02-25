using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.Email.BLL.MediatR.Feedback
{
    public class SendFeedbackCommandValidator : AbstractValidator<SendFeedbackCommand>
    {
        public SendFeedbackCommandValidator()
        {
            RuleFor(x => x.Feedback)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new FeedbackDTOValidator());
        }
    }
}
