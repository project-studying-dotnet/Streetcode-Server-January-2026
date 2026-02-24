using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.GetAll
{
    public record GetAllHistoricalContextQuery : IRequest<Result<IEnumerable<HistoricalContextDTO>>>;
}
