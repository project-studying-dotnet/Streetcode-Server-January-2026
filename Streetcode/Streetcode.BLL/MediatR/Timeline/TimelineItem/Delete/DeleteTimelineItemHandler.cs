using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public class DeleteTimelineItemHandler : IRequestHandler<DeleteTimelineItemCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteTimelineItemHandler(ILoggerService logger, IRepositoryWrapper repositoryWrapper)
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteTimelineItemCommand request, CancellationToken cancellationToken)
        {
            var timelineItem = await _repositoryWrapper.TimelineRepository
                    .GetFirstOrDefaultAsync(t => t.Id == request.Id);

            if (timelineItem is null)
            {
                var errorMsg = string.Format(Messages.Error_EntityWithIdNotFound, request.Id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            _repositoryWrapper.TimelineRepository.Delete(timelineItem);
            await _repositoryWrapper.SaveChangesAsync();

            return Result.Ok(Unit.Value);
        }
    }
}
