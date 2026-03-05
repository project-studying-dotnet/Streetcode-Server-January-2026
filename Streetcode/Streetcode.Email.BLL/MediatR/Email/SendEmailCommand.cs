using FluentResults;
using MediatR;
using Streetcode.Email.BLL.DTO;

namespace Streetcode.Email.BLL.MediatR.Email
{
    public record SendEmailCommand(EmailDTO email) : IRequest<Result<Unit>>;
}
