using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public class DeleteTimelineItemHandler : IRequestHandler<DeleteTimelineItemCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteTimelineItemHandler(
            IRepositoryWrapper repositoryWrapper,
            ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteTimelineItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var timelineItem = await _repositoryWrapper.TimelineRepository.GetFirstOrDefaultAsync(
                    x => x.Id == request.id,
                    include: i => i.Include(ti => ti.HistoricalContextTimelines));

                if (timelineItem == null)
                {
                    var errorMsg = $"Cannot find a timeline item with an ID: {request.id}";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                _repositoryWrapper.HistoricalContextTimelineRepository.DeleteRange(timelineItem.HistoricalContextTimelines);

                _repositoryWrapper.TimelineRepository.Delete(timelineItem);

                var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

                if (isSuccess)
                {
                    return Result.Ok(Unit.Value);
                }
                else
                {
                    var errorMsg = "Failed to delete the timeline item.";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }
    }
}