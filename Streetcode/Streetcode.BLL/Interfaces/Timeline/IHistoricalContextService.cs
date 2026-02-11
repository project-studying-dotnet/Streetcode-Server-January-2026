using FluentResults;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.DAL.Entities.Timeline;

namespace Streetcode.BLL.Interfaces.Timeline
{
    public interface IHistoricalContextService
    {
        Task<Result> CheckForDuplicateTitlesAsync(IEnumerable<HistoricalContextCreateDTO> contexts);
        Task<Result> BuildHistoricalContextLinksAsync(TimelineItem timelineItem, IEnumerable<HistoricalContextCreateDTO> contexts);
        Result RemoveObsoleteLinks(TimelineItem timelineItem, IEnumerable<HistoricalContextCreateDTO> newContexts);
    }
}