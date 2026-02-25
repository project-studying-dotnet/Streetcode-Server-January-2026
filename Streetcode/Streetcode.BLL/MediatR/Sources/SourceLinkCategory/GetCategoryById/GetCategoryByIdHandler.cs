using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoryById;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, Result<SourceLinkCategoryDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetCategoryByIdHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        IBlobService blobService,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<SourceLinkCategoryDTO>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var srcCategory = await _repositoryWrapper
            .SourceCategoryRepository.GetFirstOrDefaultAsync(
                predicate: sc => sc.Id == request.Id,
                include: scl => scl
                    .Include(sc => sc.StreetcodeCategoryContents)
                    .Include(sc => sc.Image) !);

        if (srcCategory is null)
        {
            var errorMsg = Messages.Error_EntityWithIdNotFound.Format(
                nameof(DAL.Entities.Sources.SourceLinkCategory),
                request.Id);

            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var mappedSrcCategory = _mapper.Map<SourceLinkCategoryDTO>(srcCategory);
        var imageBase64 = await _blobService.FindFileInStorageAsBase64(mappedSrcCategory.Image.BlobName);
        if (imageBase64 is not null)
        {
            mappedSrcCategory.Image.Base64 = imageBase64;
            return Result.Ok(mappedSrcCategory);
        }

        var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
            nameof(DAL.Entities.Media.Images.Image),
            mappedSrcCategory.Image.BlobName);

        _logger.LogError(request, errorNotFoundMsg);
        return Result.Fail(new Error(errorNotFoundMsg));
    }
}