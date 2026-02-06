using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class GetNewsAndLinksByUrlHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetNewsAndLinksByUrlHandler _handler;

        public GetNewsAndLinksByUrlHandlerTests()
        {
            _blobServiceMock = new Mock<IBlobService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new GetNewsAndLinksByUrlHandler(
                _mapperMock.Object,
                _repositoryWrapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsNotExists()
        {
            var url = "test";
            var request = new GetNewsAndLinksByUrlQuery(url);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync((NewsEntity)null);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns((NewsDTO)null);

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(e => e.Message == $"No news by entered Url - {url}");
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDTOWithURLsAndImage_WhenNewsExists()
        {
            var targetId = 2;
            var url = "target-url";
            var expectedBase64 = "fake_base64_string";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, URL = "prev-url" },
                new NewsEntity { Id = targetId, URL = url, Image = new Image() },
                new NewsEntity { Id = 3, URL = "next-url" },
            };

            var targetNewsEntity = allNews[1];
            var targetNewsDto = new NewsDTO { Id = targetId, URL = url, Image = new ImageDTO() };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(null, null))
                .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(targetNewsDto);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(expectedBase64);

            var request = new GetNewsAndLinksByUrlQuery(url);
            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.News.Image.Base64.Should().Be(expectedBase64);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Once);
            res.Value.PrevNewsUrl.Should().Be("prev-url");
            res.Value.NextNewsUrl.Should().Be("next-url");
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDTOWithURLsAndWithoutNextAndPrevNews_WhenNewsExists()
        {
            var targetId = 2;
            var url = "target-url";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = targetId, URL = url },
            };

            var targetNewsEntity = allNews[0];
            var targetNewsDto = new NewsDTO { Id = targetId, URL = url };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(null, null))
                .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(targetNewsDto);

            var request = new GetNewsAndLinksByUrlQuery(url);
            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Never);
            res.Value.PrevNewsUrl.Should().BeNull();
            res.Value.NextNewsUrl.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDTOWithURLsAndRandomNewsIsCurrentsNews_WhenNewsExistsAndNewLess3()
        {
            var targetId = 2;
            var url = "target-url";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, URL = "prev-url" },
                new NewsEntity { Id = targetId, Title = "test", URL = url,},
            };

            var targetNewsEntity = allNews[1];
            var targetNewsDto = new NewsDTO { Id = targetId, Title = "test", URL = url };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(null, null))
                .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(targetNewsDto);

            var request = new GetNewsAndLinksByUrlQuery(url);
            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.PrevNewsUrl.Should().Be("prev-url");
            res.Value.RandomNews.RandomNewsUrl.Should().Be(targetNewsDto.URL);
            res.Value.RandomNews.Title.Should().Be(targetNewsDto.Title);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDTOWithURLsAndRandomNewsIsPrevNews_WhenNewsExistsAndNewGreater3()
        {
            var targetId = 2;
            var url = "target-url";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, Title = "test1", URL = "prev-url" },
                new NewsEntity { Id = targetId, Title = "test", URL = url,},
                new NewsEntity { Id = 3, Title = "test3", URL = "next-url",},
                new NewsEntity { Id = 4, Title = "test2", URL = "test-url",},
            };

            var targetNewsEntity = allNews[1];
            var targetNewsDto = new NewsDTO { Id = targetId, Title = "test", URL = url };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(null, null))
                .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(targetNewsDto);

            var request = new GetNewsAndLinksByUrlQuery(url);
            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.RandomNews.RandomNewsUrl.Should().Be(allNews[3].URL);
            res.Value.RandomNews.Title.Should().Be(allNews[3].Title);
        }

        [Fact]
        public async Task Handle_ShouldReturnRandomNewsMinusTwo_WhenNewsIsLastAndCountGreater3()
        {
            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, Title = "First", URL = "url-1" },
                new NewsEntity { Id = 2, Title = "Second", URL = "url-2" },
                new NewsEntity { Id = 3, Title = "Third", URL = "url-3" },
                new NewsEntity { Id = 4, Title = "Last", URL = "url-4" },
            };

            var targetId = 4;
            var url = "url-4";

            var targetNewsEntity = allNews[3];
            var targetNewsDto = new NewsDTO { Id = targetId, Title = "Last", URL = url };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(null, null))
                .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(targetNewsDto);

            var request = new GetNewsAndLinksByUrlQuery(url);

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();

            res.Value.RandomNews.RandomNewsUrl.Should().Be(allNews[1].URL);
            res.Value.RandomNews.Title.Should().Be(allNews[1].Title);
        }
    }
}
