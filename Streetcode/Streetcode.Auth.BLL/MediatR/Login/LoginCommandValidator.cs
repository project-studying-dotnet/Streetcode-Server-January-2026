using FluentValidation;

namespace Streetcode.Auth.BLL.MediatR.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.LoginRequest)
                .SetValidator(new LoginRequestDTOValidator());
        }
    }
}
