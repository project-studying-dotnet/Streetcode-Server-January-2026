using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.DTO.Media.Art;

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
            var arts = await _repositoryWrapper.ArtRepository.GetAllAsync(
                predicate: sc => sc.StreetcodeArts.Any(s => s.StreetcodeId == request.StreetcodeId),
                include: scl => scl.Include(sc => sc.Image) !);

            var artsDto = _mapper.Map<IEnumerable<ArtDTO>>(arts);

            foreach (var artDto in artsDto)
            {
                if (artDto.Image != null && artDto.Image.BlobName != null)
                {
                    artDto.Image.Base64 = _blobService.FindFileInStorageAsBase64(artDto.Image.BlobName);
                }
            }

            return Result.Ok(artsDto);
        }
    }
}
