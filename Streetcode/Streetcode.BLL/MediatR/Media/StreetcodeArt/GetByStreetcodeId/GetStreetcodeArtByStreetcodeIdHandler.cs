using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.StreetcodeArt.GetByStreetcodeId
{
  public class GetStreetcodeArtByStreetcodeIdHandler : IRequestHandler<GetStreetcodeArtByStreetcodeIdQuery, Result<IEnumerable<StreetcodeArtDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;

        public GetStreetcodeArtByStreetcodeIdHandler(
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

        public async Task<Result<IEnumerable<StreetcodeArtDTO>>> Handle(GetStreetcodeArtByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var arts = await _repositoryWrapper.StreetcodeArtRepository
                .GetAllAsync(
                    predicate: s => s.StreetcodeId == request.StreetcodeId,
                    include: art => art
                        .Include(a => a.Art)
                        .Include(i => i.Art.Image) !);

            if (!arts.Any())
            {
                var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
                    nameof(DAL.Entities.Media.Images.Image),
                    request.StreetcodeId);

                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var artsDto = _mapper.Map<IEnumerable<StreetcodeArtDTO>>(arts);

            foreach (var artDto in artsDto)
            {
                var imageBase64 = await _blobService.FindFileInStorageAsBase64(artDto.Art.Image.BlobName);
                if (imageBase64 is not null)
                {
                    artDto.Art.Image.Base64 = imageBase64;
                }

                var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                    nameof(DAL.Entities.Media.Images.Image),
                    artDto.Art.Image.BlobName);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            return Result.Ok(artsDto);
        }
    }
}