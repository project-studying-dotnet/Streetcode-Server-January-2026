using FluentValidation;
using Streetcode.Resources;

namespace Streetcode.Email.BLL.MediatR.Email
{
    public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
    {
        public SendEmailCommandValidator()
        {
            RuleFor(x => x.email)
                .NotNull()
                .WithMessage(Messages.Error_CommandDataRequired)
                .SetValidator(new EmailDTOValidator());
        }
    }
}
