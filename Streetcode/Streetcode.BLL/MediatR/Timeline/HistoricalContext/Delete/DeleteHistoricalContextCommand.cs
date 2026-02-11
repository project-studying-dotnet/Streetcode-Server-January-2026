using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete
{
    public record DeleteHistoricalContextCommand(int id) : IRequest<Result<HistoricalContextDTO>>;
}