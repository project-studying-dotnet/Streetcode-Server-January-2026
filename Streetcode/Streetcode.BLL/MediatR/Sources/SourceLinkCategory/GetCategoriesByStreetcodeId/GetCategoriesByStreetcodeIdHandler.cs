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

namespace Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoriesByStreetcodeId;

public class GetCategoriesByStreetcodeIdHandler : IRequestHandler<GetCategoriesByStreetcodeIdQuery, Result<IEnumerable<SourceLinkCategoryDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetCategoriesByStreetcodeIdHandler(
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

    public async Task<Result<IEnumerable<SourceLinkCategoryDTO>>> Handle(
        GetCategoriesByStreetcodeIdQuery request,
        CancellationToken cancellationToken)
    {
        var srcCategories = await _repositoryWrapper
            .SourceCategoryRepository
            .GetAllAsync(
                predicate: sc => sc.Streetcodes.Any(s => s.Id == request.StreetcodeId),
                include: scl => scl.Include(sc => sc.Image) !);

        if (!srcCategories.Any())
        {
            var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
                nameof(DAL.Entities.Sources.SourceLinkCategory),
                request.StreetcodeId);

            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var mappedSrcCategories = _mapper.Map<IEnumerable<SourceLinkCategoryDTO>>(srcCategories);

        foreach (var srcCategory in mappedSrcCategories)
        {
            var imageBase64 = await _blobService.FindFileInStorageAsBase64(srcCategory.Image.BlobName);
            if (imageBase64 is not null)
            {
                srcCategory.Image.Base64 = imageBase64;
                continue;
            }

            var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                nameof(DAL.Entities.Media.Images.Image),
                srcCategory.Image.BlobName);

            _logger.LogError(request, errorNotFoundMsg);
            return Result.Fail(new Error(errorNotFoundMsg));
        }

        return Result.Ok(mappedSrcCategories);
    }
}