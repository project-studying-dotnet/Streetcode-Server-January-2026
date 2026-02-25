using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId
{
  public class GetArtsByStreetcodeIdHandler : IRequestHandler<GetArtsByStreetcodeIdQuery, Result<IEnumerable<ArtDTO>>>
    {
        private readonly IBlobService _blobService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetArtsByStreetcodeIdHandler(
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

        public async Task<Result<IEnumerable<ArtDTO>>> Handle(GetArtsByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var arts = await _repositoryWrapper.ArtRepository
                .GetAllAsync(
                    predicate: sc => sc.StreetcodeArts.Any(s => s.StreetcodeId == request.StreetcodeId),
                    include: scl => scl
                        .Include(sc => sc.Image) !);

            if (!arts.Any())
            {
                var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
                        nameof(DAL.Entities.Media.Images.Art),
                        request.StreetcodeId);

                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var artsDto = _mapper.Map<IEnumerable<ArtDTO>>(arts);

            foreach (var artDto in artsDto)
            {
                if (artDto.Image?.BlobName == null)
                {
                    continue;
                }

                var imageBase64 = await _blobService.FindFileInStorageAsBase64(artDto.Image.BlobName);
                if (imageBase64 is not null)
                {
                    artDto.Image.Base64 = imageBase64;
                }

                var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                    nameof(DAL.Entities.Media.Images.Image),
                    artDto.Image.BlobName);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            return Result.Ok(artsDto);
        }
    }
}
