using Streetcode.Email.BLL.DTO;

namespace Streetcode.Email.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(FeedbackDTO feedback);
    }
}