using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Create
{
    public record CreateCommentCommand(CreateCommentDTO Comment, string UserId) : IRequest<Result<CommentDTO>>;
}
