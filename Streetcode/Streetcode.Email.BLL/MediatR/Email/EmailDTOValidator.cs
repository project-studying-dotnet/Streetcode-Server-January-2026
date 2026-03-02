using FluentValidation;
using Streetcode.Email.BLL.DTO;

namespace Streetcode.Email.BLL.MediatR.Email
{
    public class EmailDTOValidator : AbstractValidator<EmailDTO>
    {
        public EmailDTOValidator()
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
