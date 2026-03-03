using FluentValidation;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public class ChangePasswordRequestDTOValidator : AbstractValidator<ChangePasswordRequestDTO>
    {
        public ChangePasswordRequestDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address format.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
                .NotEqual(x => x.CurrentPassword).WithMessage("New password cannot be the same as the current password.");
        }
    }
}