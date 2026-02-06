using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetByUrl;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class GetNewsByUrlHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetNewsByUrlHandler _handler;

        public GetNewsByUrlHandlerTests()
        {
            _blobServiceMock = new Mock<IBlobService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new GetNewsByUrlHandler(
                _mapperMock.Object,
                _repositoryWrapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsNotExists()
        {
            var url = "test";
            var request = new GetNewsByUrlQuery(url);

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
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewsWithImageExists()
        {
            var now = DateTime.Now;

            var newsDto = new NewsDTO
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = now,
                Image = new ImageDTO(),
            };

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
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(newsDto);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(expectedBase64);

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.Image.Base64.Should().Be(expectedBase64);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithOutImage_WhenNewsExistsButImageNot()
        {
            var now = DateTime.Now;

            var newsDto = new NewsDTO
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = now,
            };

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = now,
            };

            var url = "test";
            var request = new GetNewsByUrlQuery(url);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(newsDto);

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.Should().BeEquivalentTo(newsDto);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Never);
        }
    }
}
