using MediatR;
using FluentResults;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Create
{
    public record CreateTextCommand(TextBaseDTO Text) : IRequest<Result<TextDTO>>;
}
