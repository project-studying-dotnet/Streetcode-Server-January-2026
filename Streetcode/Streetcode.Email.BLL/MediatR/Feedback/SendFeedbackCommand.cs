using FluentResults;
using MediatR;
using Streetcode.Email.BLL.DTO;

namespace Streetcode.Email.BLL.MediatR.Feedback
{
    public record SendFeedbackCommand(EmailDTO Feedback) : IRequest<Result<Unit>>;
}
