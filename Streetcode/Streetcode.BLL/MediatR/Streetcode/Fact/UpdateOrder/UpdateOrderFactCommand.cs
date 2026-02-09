using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public record UpdateOrderFactCommand(List<UpdateFactOrderDTO> Facts) : IRequest<Result<Unit>>;
}
