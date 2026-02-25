using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Update
{
    public record UpdateCommentCommand(UpdateCommentDTO Comment, string UserId) : IRequest<Result<CommentDTO>>;
}
