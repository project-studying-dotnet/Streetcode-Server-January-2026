using FluentValidation;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.LoginWithGoogle
{
    public class LoginWithGoogleDTOValidator : AbstractValidator<LoginWithGoogleDTO>
    {
        public LoginWithGoogleDTOValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname is required.");
        }
    }
}
