using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.News;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class GetNewsByUrlHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly IMapper _mapper;
        private readonly GetNewsByUrlHandler _handler;

        public GetNewsByUrlHandlerTests()
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

            _handler = new GetNewsByUrlHandler(
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
            var request = new GetNewsByUrlQuery(url);

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
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewsWithImageExists()
        {
            // Arrange
            var now = DateTime.Now;

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = now,
                Image = new Image(),
            };

            var expectedBase64 = "fake_base64_string";
            var url = "test";
            var request = new GetNewsByUrlQuery(url);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    false))
                .ReturnsAsync(news);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync(expectedBase64);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            res.Value.Image.Base64.Should().Be(expectedBase64);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithOutImage_WhenNewsExistsButImageNot()
        {
            // Arrange
            var now = DateTime.Now;

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = now,
            };

            var url = "https://github.com/";
            var request = new GetNewsByUrlQuery(url);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    false))
                .ReturnsAsync(news);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsWithImageExistsButBlobNotExists()
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
            var request = new GetNewsByUrlQuery(url);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    false))
                .ReturnsAsync(news);

            // Act
            var res = await _handler.Handle(request, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(
                Messages.Error_MediaBlobNotFound.Format(nameof(Image), news.Image.BlobName));
        }
    }
}
