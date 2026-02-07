using MediatR;
using FluentResults;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create
{
    public record CreateTextCommand(TextCreateDTO Text) : IRequest<Result<TextDTO>>;
}
