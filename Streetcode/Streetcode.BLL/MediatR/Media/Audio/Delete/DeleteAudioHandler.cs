using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Audio.Delete;

public class DeleteAudioHandler : IRequestHandler<DeleteAudioCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public DeleteAudioHandler(IRepositoryWrapper repositoryWrapper, IBlobService blobService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteAudioCommand request, CancellationToken cancellationToken)
    {
        var audio = await _repositoryWrapper.AudioRepository.GetFirstOrDefaultAsync(a => a.Id == request.Id);

        if (audio is null)
        {
            var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.Media.Audio), request.Id);
            _logger.LogError(request, errorNotFoundMsg);
            return Result.Fail(new Error(errorNotFoundMsg));
        }

        _repositoryWrapper.AudioRepository.Delete(audio);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            _blobService.DeleteFileInStorage(audio.BlobName);
        }

        if (resultIsSuccess)
        {
            var successMsg = Messages.Info_EntityDeleteSuccess.Format(nameof(DAL.Entities.Media.Audio), request.Id);
            _logger?.LogInformation(successMsg);
            return Result.Ok(Unit.Value);
        }

        var errorMsg = Messages.Error_FailedToDeleteEntity.Format(nameof(DAL.Entities.Media.Audio));
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
