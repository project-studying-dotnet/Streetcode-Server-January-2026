using System.Net;

namespace Streetcode.Auth.BLL.Exceptions
{
    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message)
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }
}
