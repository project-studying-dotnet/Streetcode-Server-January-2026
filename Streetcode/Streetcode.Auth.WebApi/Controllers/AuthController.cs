using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.MediatR.Login;
using Streetcode.Auth.BLL.MediatR.LoginWithGoogle;
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

        [HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Google authentication failed" });
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);

            var command = new LoginWithGoogleCommand(new LoginWithGoogleDTO()
            {
                Email = email,
                Name = name
            });

            var loginResult = await _mediator.Send(command);

            if (loginResult.IsFailed)
            {
                return Unauthorized(loginResult.Errors.Select(e => e.Message));
            }

            var (responseDto, refreshToken) = loginResult.Value;

            _cookieService.SetRefreshTokenCookie(Response, refreshToken);

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
    }
}
