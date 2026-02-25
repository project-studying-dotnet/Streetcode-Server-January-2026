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

namespace Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl
{
    public class GetNewsAndLinksByUrlHandler : IRequestHandler<GetNewsAndLinksByUrlQuery, Result<NewsDTOWithURLs>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;
        public GetNewsAndLinksByUrlHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobService blobService, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<NewsDTOWithURLs>> Handle(GetNewsAndLinksByUrlQuery request, CancellationToken cancellationToken)
        {
            var newsDTO = _mapper.Map<NewsDTO>(
                await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                    predicate: sc => sc.URL == request.Url,
                    include: scl => scl.Include(sc => sc.Image) !));

            if (newsDTO is null)
            {
                var errorMsg = Messages.Error_NewsWithUrlNotFound.Format(nameof(DAL.Entities.News.News));
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (newsDTO.Image is not null)
            {
                var imageBase64 = await _blobService.FindFileInStorageAsBase64(newsDTO.Image.BlobName);
                if (imageBase64 is not null)
                {
                    newsDTO.Image.Base64 = imageBase64;
                }

                var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
                    nameof(DAL.Entities.Media.Images.Image),
                    newsDTO.Image.BlobName);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            var news = (await _repositoryWrapper.NewsRepository.GetAllAsync()).ToList();
            var newsIndex = news.FindIndex(x => x.Id == newsDTO.Id);
            string prevNewsLink = null;
            string nextNewsLink = null;

            if (newsIndex != 0)
            {
                prevNewsLink = news[newsIndex - 1].URL;
            }

            if (newsIndex != news.Count - 1)
            {
                nextNewsLink = news[newsIndex + 1].URL;
            }

            var randomNewsTitleAndLink = new RandomNewsDTO();

            var arrCount = news.Count;
            if (arrCount > 3)
            {
                if (newsIndex + 1 == arrCount - 1 || newsIndex == arrCount - 1)
                {
                    randomNewsTitleAndLink.RandomNewsUrl = news[newsIndex - 2].URL;
                    randomNewsTitleAndLink.Title = news[newsIndex - 2].Title;
                }
                else
                {
                    randomNewsTitleAndLink.RandomNewsUrl = news[arrCount - 1].URL;
                    randomNewsTitleAndLink.Title = news[arrCount - 1].Title;
                }
            }
            else
            {
                randomNewsTitleAndLink.RandomNewsUrl = news[newsIndex].URL;
                randomNewsTitleAndLink.Title = news[newsIndex].Title;
            }

            var newsDTOWithUrls = new NewsDTOWithURLs();
            newsDTOWithUrls.RandomNews = randomNewsTitleAndLink;
            newsDTOWithUrls.News = newsDTO;
            newsDTOWithUrls.NextNewsUrl = nextNewsLink;
            newsDTOWithUrls.PrevNewsUrl = prevNewsLink;

            return Result.Ok(newsDTOWithUrls);
        }
    }
}