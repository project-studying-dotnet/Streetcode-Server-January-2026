using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Image.GetById;

public class GetImageByIdHandler : IRequestHandler<GetImageByIdQuery, Result<ImageDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetImageByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<ImageDTO>> Handle(GetImageByIdQuery request, CancellationToken cancellationToken)
    {
        var image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(
            f => f.Id == request.Id,
            include: q => q.Include(i => i.ImageDetails) !);

        if (image is null)
        {
            var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.Media.Images.Image), request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var imageDto = _mapper.Map<ImageDTO>(image);
        if (imageDto.BlobName == null)
        {
            return Result.Ok(imageDto);
        }

        var imageBase64 = await _blobService.FindFileInStorageAsBase64(imageDto.BlobName);
        if (imageBase64 is not null)
        {
            imageDto.Base64 = imageBase64;
            return Result.Ok(imageDto);
        }

        var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
            nameof(DAL.Entities.Media.Images.Image),
            imageDto.BlobName);

        _logger.LogError(request, errorNotFoundMsg);
        return Result.Fail(new Error(errorNotFoundMsg));
    }
}