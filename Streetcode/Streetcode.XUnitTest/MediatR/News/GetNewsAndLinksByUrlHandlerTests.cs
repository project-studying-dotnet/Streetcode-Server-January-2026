using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.News;
using Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class GetNewsAndLinksByUrlHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly IMapper _mapper;
        private readonly GetNewsAndLinksByUrlHandler _handler;

        public GetNewsAndLinksByUrlHandlerTests()
        {
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();

            var config = new MapperConfiguration(conf =>
            {
                conf.AddProfile(new NewsProfile());
                conf.AddProfile(new ImageProfile());
            });

            _mapper = config.CreateMapper();

            _handler = new GetNewsAndLinksByUrlHandler(
                _mapper,
                _repositoryWrapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsNotExists()
        {
            // Arrange
            var url = "test";
            var request = new GetNewsAndLinksByUrlQuery(url);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false))
                .ReturnsAsync((NewsEntity)null);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(Messages.Error_NewsWithUrlNotFound.Format(url));
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDTOWithURLsAndImage_WhenNewsExists()
        {
            // Arrange
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

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                null,
                null,
                false))
                .ReturnsAsync(allNews);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync(expectedBase64);

            var request = new GetNewsAndLinksByUrlQuery(url);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);


            // Assert
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
            // Arrange
            var targetId = 2;
            var url = "target-url";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = targetId, URL = url },
            };

            var targetNewsEntity = allNews[0];

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                null,
                null,
                false))
                .ReturnsAsync(allNews);

            var request = new GetNewsAndLinksByUrlQuery(url);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
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
            // Arrange
            var targetId = 2;
            var url = "target-url";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, URL = "prev-url" },
                new NewsEntity { Id = targetId, Title = "test", URL = url,},
            };

            var targetNewsEntity = allNews[1];

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                null,
                null,
                false))
                .ReturnsAsync(allNews);

            var request = new GetNewsAndLinksByUrlQuery(url);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            res.Value.PrevNewsUrl.Should().Be("prev-url");
            res.Value.RandomNews.RandomNewsUrl.Should().Be(targetNewsEntity.URL);
            res.Value.RandomNews.Title.Should().Be(targetNewsEntity.Title);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDTOWithURLsAndRandomNewsIsPrevNews_WhenNewsExistsAndNewGreater3()
        {
            // Arrange
            var targetId = 2;
            var url = "target-url";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, Title = "test1", URL = "prev-url" },
                new NewsEntity { Id = targetId, Title = "test", URL = url, },
                new NewsEntity { Id = 3, Title = "test3", URL = "next-url", },
                new NewsEntity { Id = 4, Title = "test2", URL = "test-url", },
            };

            var targetNewsEntity = allNews[1];

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                null,
                null,
                false))
                .ReturnsAsync(allNews);

            var request = new GetNewsAndLinksByUrlQuery(url);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);


            // Assert
            res.IsSuccess.Should().BeTrue();
            res.Value.RandomNews.RandomNewsUrl.Should().Be(allNews[3].URL);
            res.Value.RandomNews.Title.Should().Be(allNews[3].Title);
        }

        [Fact]
        public async Task Handle_ShouldReturnRandomNewsMinusTwo_WhenNewsIsLastAndCountGreater3()
        {
            // Arrange
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

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false))
                .ReturnsAsync(targetNewsEntity);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                null,
                null,
                false))
                .ReturnsAsync(allNews);

            var request = new GetNewsAndLinksByUrlQuery(url);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            res.Value.RandomNews.RandomNewsUrl.Should().Be(allNews[1].URL);
            res.Value.RandomNews.Title.Should().Be(allNews[1].Title);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsFoundWithImageButBlobNotExists()
        {
            // Arrange
            var now = DateTime.Now;

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = now,
                Image = new Image
                {
                    BlobName = "BlobName",
                },
            };

            var url = "https://github.com/";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    false))
                .ReturnsAsync(news);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            // Act
            var res = await _handler.Handle(new GetNewsAndLinksByUrlQuery(url), CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(Messages.Error_MediaBlobNotFound.Format(nameof(Image), news.Image.BlobName));
        }
    }
}
