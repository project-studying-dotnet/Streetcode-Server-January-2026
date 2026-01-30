using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Update
{
    public record UpdateTextCommand(TextBaseDTO Text) : IRequest<Result<TextDTO>>;
}
