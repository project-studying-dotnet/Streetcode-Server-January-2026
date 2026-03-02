using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create
{
    public record CreateHistoricalContextCommand(CreateHistoricalContextDTO HistoricalContext) : IRequest<Result<HistoricalContextDTO>>;
}
