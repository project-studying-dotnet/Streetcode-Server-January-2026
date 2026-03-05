using FluentValidation;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Request)
                .SetValidator(new ChangePasswordRequestDTOValidator());

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("User email is missing from the identity claim.")
                .EmailAddress();
        }
    }
}