namespace Streetcode.Auth.WebApi.Services.Interfaces
{
    public interface IRefreshTokenCookieService
    {
        void SetRefreshTokenCookie(HttpResponse response, string token);
        string? GetRefreshTokenFromRequest(HttpRequest request);
        void DeleteRefreshTokenCookie(HttpResponse response);
    }
}
