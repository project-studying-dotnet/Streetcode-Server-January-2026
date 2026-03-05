using FluentValidation;
using Streetcode.Email.BLL.DTO;

namespace Streetcode.Email.BLL.MediatR.Email
{
    public class EmailDTOValidator : AbstractValidator<EmailDTO>
    {
        public EmailDTOValidator()
        { 
            RuleFor(x => x.From)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Content)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(1000);
        }
    }
}
