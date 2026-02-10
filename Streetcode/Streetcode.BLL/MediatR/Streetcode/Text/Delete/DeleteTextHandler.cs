using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Delete
{
    public class DeleteTextHandler : IRequestHandler<DeleteTextCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteTextHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteTextCommand request, CancellationToken cancellationToken)
        {
            var text = await _repositoryWrapper.TextRepository
                .GetFirstOrDefaultAsync(t => t.Id == request.Id);

            if (text is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(DAL.Entities.Streetcode.TextContent.Text),
                    request.Id);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            _repositoryWrapper.TextRepository.Delete(text);

            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(Unit.Value);
            }

            var errorMsg = Messages.Error_FailedToDeleteEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.Text));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}