using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete
{
    public class DeleteHistoricalContextHandler : IRequestHandler<DeleteHistoricalContextCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteHistoricalContextHandler(ILoggerService logger, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteHistoricalContextCommand request, CancellationToken cancellationToken)
        {
            var historicalContext = await _repositoryWrapper.HistoricalContextRepository
                    .GetFirstOrDefaultAsync(hc => hc.Id == request.Id);

            if (historicalContext is null)
            {
                var errorMsg = string.Format(Messages.Error_EntityWithIdNotFound, request.Id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            _repositoryWrapper.HistoricalContextRepository.Delete(historicalContext);
            await _repositoryWrapper.SaveChangesAsync();

            return Result.Ok(Unit.Value);
        }
    }
}
