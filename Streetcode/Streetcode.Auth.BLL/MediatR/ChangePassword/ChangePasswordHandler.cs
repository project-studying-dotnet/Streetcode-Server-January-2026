using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result<Unit>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Request.Email);

            if (user == null)
            {
                return Result.Fail("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.Request.CurrentPassword,
                request.Request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return Result.Fail(string.Join(", ", errors));
            }

            return Result.Ok(Unit.Value);
        }
    }
}