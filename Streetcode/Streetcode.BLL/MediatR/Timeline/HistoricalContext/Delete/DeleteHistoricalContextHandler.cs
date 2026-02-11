using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete
{
    public class DeleteHistoricalContextHandler : IRequestHandler<DeleteHistoricalContextCommand, Result<HistoricalContextDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public DeleteHistoricalContextHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<HistoricalContextDTO>> Handle(DeleteHistoricalContextCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var historicalContext = await _repositoryWrapper.HistoricalContextRepository
                    .GetSingleOrDefaultAsync(
                        predicate: hc => hc.Id == request.id,
                        include: i => i.Include(hc => hc.HistoricalContextTimelines));

                if (historicalContext == null)
                {
                    string errorMsg = $"Cannot find historical context with an ID: {request.id}";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                if (historicalContext.HistoricalContextTimelines.Any())
                {
                    const string errorMsg = "Cannot delete a historical context that is in use by a timeline item.";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                _repositoryWrapper.HistoricalContextRepository.Delete(historicalContext);

                var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
                if (isSuccess)
                {
                    var resultDto = _mapper.Map<HistoricalContextDTO>(historicalContext);
                    return Result.Ok(resultDto);
                }
                else
                {
                    const string errorMsg = "Failed to delete the historical context.";
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