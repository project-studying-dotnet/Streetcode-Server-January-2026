using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.News.Update
{
    public class UpdateNewsHandler : IRequestHandler<UpdateNewsCommand, Result<NewsDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;
        public UpdateNewsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<NewsDTO>> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
        {
            if (request.News is null)
            {
                var errorConvertMsg = Messages.Error_ConvertNullToEntity.Format(nameof(DAL.Entities.News.News));
                _logger.LogError(request, errorConvertMsg);
                return Result.Fail(new Error(errorConvertMsg));
            }

            var newsEntity = await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                    n => n.Id == request.News.Id,
                    x => x.Include(n => n.Image),
                    true);

            if (newsEntity is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(DAL.Entities.News.News),
                    request.News.Id);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            if (request.News.Image is not null && newsEntity.ImageId != request.News.ImageId)
            {
                var img = await _repositoryWrapper.ImageRepository
                    .GetFirstOrDefaultAsync(
                        x => x.Id == request.News.Image.Id,
                        trackEntities: true);

                if (img is null)
                {
                    var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                        nameof(DAL.Entities.Media.Images.Image),
                        request.News.Image.Id);

                    _logger.LogError(request, errorNotFoundMsg);
                    return Result.Fail(errorNotFoundMsg);
                }

                var imageBase64 = await _blobService.FindFileInStorageAsBase64(request.News.Image.BlobName);
                if (imageBase64 is null)
                {
                    var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                        nameof(DAL.Entities.Media.Images.Image),
                        request.News.Image.BlobName);

                    _logger.LogError(request, errorNotFoundMsg);
                    return Result.Fail(new Error(errorNotFoundMsg));
                }

                request.News.Image.Base64 = imageBase64;
                _repositoryWrapper.ImageRepository.Delete(newsEntity.Image);
            }
            else if (request.News.Image is null)
            {
                _repositoryWrapper.ImageRepository.Delete(newsEntity.Image);
            }

            _mapper.Map(request.News, newsEntity);

            _repositoryWrapper.NewsRepository.Update(newsEntity);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<NewsDTO>(newsEntity));
            }

            var errorMsg = Messages.Error_FailedToUpdateEntity.Format(nameof(DAL.Entities.News.News));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
