using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.GetPending
{
    public record GetPendingCommentsQuery() : IRequest<Result<IEnumerable<CommentDTO>>>;
}
