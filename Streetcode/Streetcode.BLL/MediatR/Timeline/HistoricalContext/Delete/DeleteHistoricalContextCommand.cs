using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete
{
    public record DeleteHistoricalContextCommand(int Id) : IRequest<Result<Unit>>;
}
