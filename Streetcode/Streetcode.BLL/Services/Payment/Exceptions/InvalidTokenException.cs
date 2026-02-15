using Streetcode.Resources;

namespace Streetcode.BLL.Services.Payment.Exceptions
{
    /// <summary>
    /// Represents error caused by invalid Token value.
    /// </summary>
    public class InvalidTokenException : MonobankException
    {
        internal InvalidTokenException()
            : base(Messages.Error_InvalidMonobankTokenException)
        {
        }
    }
}
