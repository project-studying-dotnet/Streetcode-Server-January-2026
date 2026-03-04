using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus
{
    public record UpdateCommentStatusCommand(UpdateCommentStatusDTO Comment) : IRequest<Result<CommentDTO>>;
}
