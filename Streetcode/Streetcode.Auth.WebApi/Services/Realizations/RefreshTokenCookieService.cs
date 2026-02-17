using Streetcode.Auth.WebApi.Services.Interfaces;

namespace Streetcode.Auth.WebApi.Services.Realizations
{
    public class RefreshTokenCookieService : IRefreshTokenCookieService
    {
        private const string CookieName = "refreshToken";
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public RefreshTokenCookieService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void SetRefreshTokenCookie(HttpResponse response, string token)
        {
            var cookieOptions = GetCookieOptions();
            response.Cookies.Append(CookieName, token, cookieOptions);
        }

        public string? GetRefreshTokenFromRequest(HttpRequest request)
        {
            return request.Cookies[CookieName];
        }

        public void DeleteRefreshTokenCookie(HttpResponse response)
        {
            var cookieOptions = GetCookieOptions();
            response.Cookies.Delete(CookieName, cookieOptions);
        }

        private CookieOptions GetCookieOptions()
        {
            var sameSiteMode = _environment.IsProduction() ? SameSiteMode.None : SameSiteMode.Lax;

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = sameSiteMode,
                Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpireDays"] !))
            };
        }
    }
}
