using MediatR;
using FluentResults;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public record CreateTimelineItemCommand(int StreetcodeId, TimelineItemCreateDTO TimelineItem) : IRequest<Result<TimelineItemDTO>>;
}