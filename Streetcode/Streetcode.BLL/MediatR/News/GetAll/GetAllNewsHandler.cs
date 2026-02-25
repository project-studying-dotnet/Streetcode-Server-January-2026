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

namespace Streetcode.BLL.MediatR.News.GetAll
{
    public class GetAllNewsHandler : IRequestHandler<GetAllNewsQuery, Result<IEnumerable<NewsDTO>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;

        public GetAllNewsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<NewsDTO>>> Handle(GetAllNewsQuery request, CancellationToken cancellationToken)
        {
            var news = await _repositoryWrapper.NewsRepository.GetAllAsync(
                include: cat => cat.Include(img => img.Image !));

            if (!news.Any())
            {
                var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.News.News));
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newsDTOs = _mapper.Map<IEnumerable<NewsDTO>>(news);

            foreach (var dto in newsDTOs)
            {
                if (dto.Image is null)
                {
                    continue;
                }

                var imageBase64 = await _blobService.FindFileInStorageAsBase64(dto.Image.BlobName);
                if (imageBase64 is not null)
                {
                    dto.Image.Base64 = imageBase64;
                }

                var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                    nameof(DAL.Entities.Media.Images.Image),
                    dto.Image.BlobName);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            return Result.Ok(newsDTOs);
        }
    }
}
