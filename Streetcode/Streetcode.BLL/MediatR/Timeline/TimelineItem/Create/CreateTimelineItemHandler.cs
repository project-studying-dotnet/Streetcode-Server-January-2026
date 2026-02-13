using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Timeline;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateTimelineItemHandler : IRequestHandler<CreateTimelineItemCommand, Result<TimelineItemDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        private readonly IHistoricalContextService _historicalContextService;

        public CreateTimelineItemHandler(
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

        public async Task<Result<TimelineItemDTO>> Handle(CreateTimelineItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newTimelineItem = _mapper.Map<TimelineItemEntity>(request.TimelineItem);
                if (newTimelineItem is null)
                {
                    const string errorMsg = "Cannot convert null to timeline item";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                var existingStreetcode = await _repositoryWrapper.StreetcodeRepository
                    .GetFirstOrDefaultAsync(s => s.Id == request.StreetcodeId);

                if (existingStreetcode is null)
                {
                    string errorMsg = $"Streetcode with Id={request.StreetcodeId} not found";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                newTimelineItem.StreetcodeId = request.StreetcodeId;
                newTimelineItem.HistoricalContextTimelines = new List<HistoricalContextTimeline>();

                var validationCheck = await ValidateAndBuildHistoricalContextsAsync(request, newTimelineItem);
                if (validationCheck.IsFailed)
                {
                    return validationCheck;
                }

                var entity = await _repositoryWrapper.TimelineRepository.CreateAsync(newTimelineItem);

                var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
                if (resultIsSuccess)
                {
                    return Result.Ok(_mapper.Map<TimelineItemDTO>(entity));
                }

                const string createErrorMsg = "Failed to create a timeline item";
                _logger.LogError(request, createErrorMsg);
                return Result.Fail(createErrorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }

        private async Task<Result> ValidateAndBuildHistoricalContextsAsync(
            CreateTimelineItemCommand request,
            TimelineItemEntity newTimelineItem)
        {
            var duplicateCheck = await _historicalContextService.CheckForDuplicateTitlesAsync(request.TimelineItem.HistoricalContexts!);
            if (duplicateCheck.IsFailed)
            {
                _logger.LogError(request, duplicateCheck.Errors[0].Message);
                return Result.Fail(duplicateCheck.Errors);
            }

            var linkResult = await _historicalContextService.BuildHistoricalContextLinksAsync(newTimelineItem, request.TimelineItem.HistoricalContexts!);
            if (linkResult.IsFailed)
            {
                _logger.LogError(request, linkResult.Errors[0].Message);
                return Result.Fail(linkResult.Errors);
            }

            return Result.Ok();
        }
    }
}
