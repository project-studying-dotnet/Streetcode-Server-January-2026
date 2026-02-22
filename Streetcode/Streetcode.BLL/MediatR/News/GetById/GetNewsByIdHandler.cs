using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.News.GetById
{
    public class GetNewsByIdHandler : IRequestHandler<GetNewsByIdQuery, Result<NewsDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;
        public GetNewsByIdHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobService blobService, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<NewsDTO>> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
        {
            var newsDTO = _mapper.Map<NewsDTO>(
                await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                    predicate: sc => sc.Id == request.Id,
                    include: scl => scl.Include(sc => sc.Image) !));

            if (newsDTO is null)
            {
                var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.News.News), request.Id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (newsDTO.Image is not null)
            {
                newsDTO.Image.Base64 = _blobService.FindFileInStorageAsBase64(newsDTO.Image.BlobName);
            }

            return Result.Ok(newsDTO);
        }
    }
}