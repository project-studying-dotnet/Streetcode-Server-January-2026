using FluentValidation;
using Streetcode.Auth.BLL.MediatR.Login;

namespace Streetcode.Auth.BLL.MediatR.LoginWithGoogle
{
    public class LoginWithGoogleCommandValidation : AbstractValidator<LoginWithGoogleCommand>
    {
        public LoginWithGoogleCommandValidation()
        {
            RuleFor(x => x.LoginGoogle)
                .SetValidator(new LoginWithGoogleDTOValidator());
        }
    }
}
