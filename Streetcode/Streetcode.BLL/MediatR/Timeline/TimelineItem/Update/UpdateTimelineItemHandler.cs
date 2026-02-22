using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class UpdateTimelineItemHandler : IRequestHandler<UpdateTimelineItemCommand, Result<TimelineItemDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateTimelineItemHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<TimelineItemDTO>> Handle(UpdateTimelineItemCommand request, CancellationToken cancellationToken)
        {
            var existingTimelineItem = await _repositoryWrapper.TimelineRepository
                .FindAll()
                .Include(t => t.HistoricalContextTimelines)
                .FirstOrDefaultAsync(t => t.Id == request.TimelineItem.Id);

            if (existingTimelineItem is null)
            {
                var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(TimelineItem), request.TimelineItem.Id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var streetcode = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(s => s.Id == request.TimelineItem.StreetcodeId);

            if (streetcode is null)
            {
                var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Streetcode), request.TimelineItem.StreetcodeId);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (request.TimelineItem.HistoricalContextIds.Any())
            {
                var existingContexts = await _repositoryWrapper.HistoricalContextRepository
                    .GetAllAsync(predicate: hc => request.TimelineItem.HistoricalContextIds.Contains(hc.Id));

                var existingContextIds = existingContexts.Select(hc => hc.Id).ToList();
                var missingContextIds = request.TimelineItem.HistoricalContextIds
                    .Except(existingContextIds).ToList();

                if (missingContextIds.Any())
                {
                    var errorMsg = string.Format(Messages.Error_EntityWithIdNotFound, string.Join(", ", missingContextIds));
                    _logger.LogError(request, errorMsg);
                    return Result.Fail<TimelineItemDTO>(errorMsg);
                }
            }

            _mapper.Map(request.TimelineItem, existingTimelineItem);

            var oldRelationships = existingTimelineItem.HistoricalContextTimelines.ToList();
            foreach (var oldRel in oldRelationships)
            {
                _repositoryWrapper.HistoricalContextTimelineRepository.Delete(oldRel);
            }

            existingTimelineItem.HistoricalContextTimelines = request.TimelineItem.HistoricalContextIds
                .Select(id => new HistoricalContextTimeline
                {
                    TimelineId = existingTimelineItem.Id,
                    HistoricalContextId = id
                }).ToList();

            _repositoryWrapper.TimelineRepository.Update(existingTimelineItem);
            await _repositoryWrapper.SaveChangesAsync();

            var result = await _repositoryWrapper.TimelineRepository
                .GetFirstOrDefaultAsync(
                    predicate: t => t.Id == existingTimelineItem.Id,
                    include: query => query
                        .Include(t => t.HistoricalContextTimelines)
                        .ThenInclude(hct => hct.HistoricalContext));

            return Result.Ok(_mapper.Map<TimelineItemDTO>(result));
        }
    }
}
