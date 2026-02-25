using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;

public record GetTimelineItemByIdQuery(int Id) : IRequest<Result<TimelineItemDTO>>;
