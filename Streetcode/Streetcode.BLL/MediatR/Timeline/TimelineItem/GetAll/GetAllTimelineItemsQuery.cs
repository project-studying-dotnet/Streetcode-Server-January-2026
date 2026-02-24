using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;

public record GetAllTimelineItemsQuery : IRequest<Result<IEnumerable<TimelineItemDTO>>>;