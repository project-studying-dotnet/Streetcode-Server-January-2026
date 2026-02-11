using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class UpdateTimelineItemHandler : IRequestHandler<UpdateTimelineItemCommand, Result<TimelineItemDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        private readonly IHistoricalContextService _historicalContextService;

        public UpdateTimelineItemHandler(
            IMapper mapper,
            IRepositoryWrapper repositoryWrapper,
            ILoggerService logger,
            IHistoricalContextService historicalContextService)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _historicalContextService = historicalContextService;
        }

        public async Task<Result<TimelineItemDTO>> Handle(UpdateTimelineItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var timelineItem = await _repositoryWrapper.TimelineRepository.GetFirstOrDefaultAsync(
                    predicate: x => x.Id == request.TimelineItem.Id,
                    include: i => i.Include(ti => ti.HistoricalContextTimelines));

                if (timelineItem == null)
                {
                    string errorMsg = $"Cannot find timeline item with Id={request.TimelineItem.Id}";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                _mapper.Map(request.TimelineItem, timelineItem);

                var contextUpdateResult = await UpdateHistoricalContextsAsync(request, timelineItem);
                if (contextUpdateResult.IsFailed)
                {
                    return contextUpdateResult;
                }

                _repositoryWrapper.TimelineRepository.Update(timelineItem);

                var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
                if (resultIsSuccess)
                {
                    return Result.Ok(_mapper.Map<TimelineItemDTO>(timelineItem));
                }
                else
                {
                    const string errorMsg = "Failed to save changes to the database";
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

        private async Task<Result> UpdateHistoricalContextsAsync(
            UpdateTimelineItemCommand request,
            TimelineItemEntity timelineItem)
        {
            var newContexts = request.TimelineItem.HistoricalContexts!;

            var duplicateCheckResult = await _historicalContextService.CheckForDuplicateTitlesAsync(newContexts);
            if (duplicateCheckResult.IsFailed)
            {
                _logger.LogError(request, duplicateCheckResult.Errors[0].Message);
                return Result.Fail(duplicateCheckResult.Errors[0].Message);
            }

            var removalResult = _historicalContextService.RemoveObsoleteLinks(timelineItem, newContexts);
            if (removalResult.IsFailed)
            {
                _logger.LogError(request, removalResult.Errors[0].Message);
                return Result.Fail(removalResult.Errors[0].Message);
            }

            var linkResult = await _historicalContextService.BuildHistoricalContextLinksAsync(timelineItem, newContexts);
            if (linkResult.IsFailed)
            {
                _logger.LogError(request, linkResult.Errors[0].Message);
                return Result.Fail(linkResult.Errors[0].Message);
            }

            return Result.Ok();
        }
    }
}