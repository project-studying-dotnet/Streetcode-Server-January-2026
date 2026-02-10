using System.Net;
namespace Streetcode.Auth.BLL.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
