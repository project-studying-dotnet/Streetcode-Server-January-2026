using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public record DeleteTimelineItemCommand(int id) : IRequest<Result<Unit>>;
}