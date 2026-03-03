using MediatR;
using Microsoft.AspNetCore.Mvc;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.MediatR.ChangePassword;
using Streetcode.Auth.BLL.MediatR.Login;
using Streetcode.Auth.BLL.MediatR.Logout;
using Streetcode.Auth.BLL.MediatR.RefreshToken;
using Streetcode.Auth.BLL.MediatR.Register;
using Streetcode.Auth.WebApi.Services.Interfaces;

namespace Streetcode.Auth.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IRefreshTokenCookieService _cookieService;

        public AuthController(IMediator mediator, IRefreshTokenCookieService cookieService)
        {
            _mediator = mediator;
            _cookieService = cookieService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequest)
        {
            var result = await _mediator.Send(new RegisterCommand(registerRequest));
            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            var (responseDto, refreshToken) = result.Value;

            _cookieService.SetRefreshTokenCookie(Response, refreshToken);

            return Ok(responseDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            var result = await _mediator.Send(new LoginCommand(loginRequest));
            if (result.IsFailed)
            {
                return Unauthorized(result.Errors.Select(e => e.Message));
            }

            var (responseDto, refreshToken) = result.Value;

            _cookieService.SetRefreshTokenCookie(Response, refreshToken);

            return Ok(responseDto);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = _cookieService.GetRefreshTokenFromRequest(Request);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No token");
            }

            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));
            if (result.IsFailed)
            {
                return Unauthorized(result.Errors.Select(e => e.Message));
            }

            var (responseDto, newRefreshToken) = result.Value;

            _cookieService.SetRefreshTokenCookie(Response, newRefreshToken);

            return Ok(responseDto);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = _cookieService.GetRefreshTokenFromRequest(Request);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _mediator.Send(new LogoutCommand(refreshToken));
            }

            _cookieService.DeleteRefreshTokenCookie(Response);

            return Ok(new { message = "Logged out" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            var result = await _mediator.Send(new ChangePasswordCommand(request));

            if (result.IsSuccess)
            {
                return Ok("Password changed successfully");
            }

            return BadRequest(result.Errors[0].Message);
        }
    }
}
