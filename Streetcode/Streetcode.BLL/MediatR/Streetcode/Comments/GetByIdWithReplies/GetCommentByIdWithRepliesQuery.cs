using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.GetByIdWithReplies
{
    public record GetCommentByIdWithRepliesQuery(int Id) : IRequest<Result<CommentDTO>>;
}
