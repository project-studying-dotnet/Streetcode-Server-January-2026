using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update
{
    public record UpdateHistoricalContextCommand(UpdateHistoricalContextDTO HistoricalContext) : IRequest<Result<HistoricalContextDTO>>;
}
