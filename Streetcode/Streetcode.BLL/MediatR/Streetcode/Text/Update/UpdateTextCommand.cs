using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public record UpdateTextCommand(TextBaseDTO Text) : IRequest<Result<TextDTO>>;
}
