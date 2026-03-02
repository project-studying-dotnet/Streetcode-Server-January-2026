using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Delete
{
    public record DeleteCommentCommand(int Id, string UserId) : IRequest<Result<Unit>>;
}
