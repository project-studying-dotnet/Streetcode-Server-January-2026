using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.AdminDelete
{
    public record AdminDeleteCommentCommand(int Id) : IRequest<Result<Unit>>;
}
