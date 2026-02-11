using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;

public record GetTimelineItemsByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<TimelineItemDTO>>>;