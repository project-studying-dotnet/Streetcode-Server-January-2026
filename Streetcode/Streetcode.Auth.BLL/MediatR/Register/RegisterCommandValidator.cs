using FluentValidation;

namespace Streetcode.Auth.BLL.MediatR.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.RegisterRequest)
                .SetValidator(new RegisterRequestDTOValidator());
        }
    }
}
