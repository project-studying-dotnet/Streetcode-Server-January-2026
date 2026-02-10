using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;

public class GetAllTimelineItemsHandler : IRequestHandler<GetAllTimelineItemsQuery, Result<IEnumerable<TimelineItemDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetAllTimelineItemsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TimelineItemDTO>>> Handle(GetAllTimelineItemsQuery request, CancellationToken cancellationToken)
    {
        var timelineItems = await _repositoryWrapper.TimelineRepository
            .GetAllAsync(
                include: ti => ti
                  .Include(til => til.HistoricalContextTimelines)
                  .ThenInclude(x => x.HistoricalContext) !);

        if (timelineItems.Any())
        {
            return Result.Ok(_mapper.Map<IEnumerable<TimelineItemDTO>>(timelineItems));
        }

        var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.Timeline.TimelineItem));
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
