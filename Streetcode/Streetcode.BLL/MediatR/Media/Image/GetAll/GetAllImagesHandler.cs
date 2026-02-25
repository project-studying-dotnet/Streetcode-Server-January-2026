using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Image.GetAll;

public class GetAllImagesHandler : IRequestHandler<GetAllImagesQuery, Result<IEnumerable<ImageDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetAllImagesHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ImageDTO>>> Handle(GetAllImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await _repositoryWrapper.ImageRepository.GetAllAsync();

        if (!images.Any())
        {
            var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.Media.Images.Image));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var imageDtos = _mapper.Map<IEnumerable<ImageDTO>>(images);

        foreach (var image in imageDtos)
        {
            var imageBase64 = await _blobService.FindFileInStorageAsBase64(image.BlobName);
            if (imageBase64 is not null)
            {
                image.Base64 = imageBase64;
            }

            var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                nameof(DAL.Entities.Media.Images.Image),
                image.BlobName);

            _logger.LogError(request, errorNotFoundMsg);
            return Result.Fail(new Error(errorNotFoundMsg));
        }

        return Result.Ok(imageDtos);
    }
}