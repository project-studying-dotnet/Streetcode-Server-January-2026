using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Image.GetBaseImage;

public class GetBaseImageHandler : IRequestHandler<GetBaseImageQuery, Result<MemoryStream>>
{
    private readonly IBlobService _blobStorage;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetBaseImageHandler(IBlobService blobService, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _blobStorage = blobService;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<MemoryStream>> Handle(GetBaseImageQuery request, CancellationToken cancellationToken)
    {
        var image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(a => a.Id == request.Id);

        if (image is not null)
        {
            var imageMemoryStream = await _blobStorage.FindFileInStorageAsMemoryStream(image.BlobName);
            if (imageMemoryStream is not null)
            {
                return Result.Ok(imageMemoryStream);
            }

            var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                nameof(DAL.Entities.Media.Images.Image),
                image.BlobName);

            _logger.LogError(request, errorNotFoundMsg);
            return Result.Fail(new Error(errorNotFoundMsg));
        }

        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.Media.Images.Image), request.Id);
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
