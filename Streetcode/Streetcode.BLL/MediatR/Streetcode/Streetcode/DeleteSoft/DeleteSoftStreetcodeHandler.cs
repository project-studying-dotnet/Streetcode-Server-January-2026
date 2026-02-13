using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.DeleteSoft;

public class DeleteSoftStreetcodeHandler : IRequestHandler<DeleteSoftStreetcodeCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheService _cacheService;

    public DeleteSoftStreetcodeHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, ICacheService cacheService)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<Unit>> Handle(DeleteSoftStreetcodeCommand request, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository
            .GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (streetcode is null)
        {
            var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(StreetcodeContent), request.Id);
            _logger.LogError(request, errorNotFoundMsg);
            throw new ArgumentNullException(errorNotFoundMsg);
        }

        streetcode.Status = DAL.Enums.StreetcodeStatus.Deleted;
        streetcode.UpdatedAt = DateTime.Now;

        _repositoryWrapper.StreetcodeRepository.Update(streetcode);

        var resultIsDeleteSucces = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsDeleteSucces)
        {
            await _cacheService.RemoveAsync($"Streetcode_{request.Id}".ToLower(), cancellationToken);

            return Result.Ok(Unit.Value);
        }

        var errorMsg = Messages.Error_FailedToSoftDeleteStreetcode;
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}