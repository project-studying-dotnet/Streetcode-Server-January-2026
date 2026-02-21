using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.GetByStreetcodeId
{
    public record GetCommentsByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<CommentDTO>>>;
}
