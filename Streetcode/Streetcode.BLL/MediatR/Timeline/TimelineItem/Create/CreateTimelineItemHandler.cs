using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using EntityTimelineItem = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateHistoricalContextHandler : IRequestHandler<CreateHistoricalContextCommand, Result<TimelineItemDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public CreateHistoricalContextHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<TimelineItemDTO>> Handle(CreateHistoricalContextCommand request, CancellationToken cancellationToken)
        {
            var streetCodeExists = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(s => s.Id == request.TimelineItem.StreetcodeId);

            if (streetCodeExists is null)
            {
                var errorNotFoundMsg = string.Format(Messages.Error_EntityWithIdNotFound, request.TimelineItem.StreetcodeId);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(errorNotFoundMsg);
            }

            if (request.TimelineItem.HistoricalContextIds.Any())
            {
                var existingContexts = await _repositoryWrapper.HistoricalContextRepository
                       .GetAllAsync(
                           predicate: hc => request.TimelineItem.HistoricalContextIds.Contains(hc.Id));

                var existingContextIds = existingContexts.Select(hc => hc.Id).ToList();
                var missingContextIds = request.TimelineItem.HistoricalContextIds
                    .Except(existingContextIds).ToList();

                if (missingContextIds.Any())
                {
                    var errorMsg = string.Format(Messages.Error_EntityWithIdNotFound, string.Join(", ", missingContextIds));
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }
            }

            var newTimelineItem = _mapper.Map<EntityTimelineItem>(request.TimelineItem);

            newTimelineItem.HistoricalContextTimelines = request.TimelineItem.HistoricalContextIds
                .Select(id => new HistoricalContextTimeline
                {
                    HistoricalContextId = id,
                    Timeline = newTimelineItem
                }).ToList();

            await _repositoryWrapper.TimelineRepository.CreateAsync(newTimelineItem);
            await _repositoryWrapper.SaveChangesAsync();

            var result = await _repositoryWrapper.TimelineRepository
                .GetFirstOrDefaultAsync(
                    t => t.Id == newTimelineItem.Id,
                    query => query
                        .Include(t => t.HistoricalContextTimelines)
                        .ThenInclude(hct => hct.HistoricalContext));

            return Result.Ok(_mapper.Map<TimelineItemDTO>(result));
        }
    }
}
