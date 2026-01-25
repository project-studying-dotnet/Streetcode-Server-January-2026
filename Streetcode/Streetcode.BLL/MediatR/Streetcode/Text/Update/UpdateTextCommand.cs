using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Update
{
    public record UpdateTextCommand(int id, TextDTO text) : IRequest<Result<Unit>>;
}
