using FluentValidation;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Request)
                .SetValidator(new ChangePasswordRequestDTOValidator());
        }
    }
}