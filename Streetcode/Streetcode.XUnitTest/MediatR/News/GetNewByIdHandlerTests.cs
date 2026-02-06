using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetById;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class GetNewByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetNewsByIdHandler _handler;

        public GetNewByIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetNewsByIdHandler(
                _mapperMock.Object,
                _repositoryWrapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoNewsById()
        {
            int id = 1;

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync((NewsEntity)null!);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns((NewsDTO)null!);

            var request = new GetNewsByIdQuery(id);

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message == $"No news by entered Id - {id}");
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDto_WhenNewsExistById()
        {
            int id = 1;

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

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(news);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(newsDto);

            var request = new GetNewsByIdQuery(id);

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(newsDto);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewsExistByIdWithImage()
        {
            int id = 1;
            var fakeBase = "fabe_base_64";

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

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(news);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(fakeBase);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>()))
                .Returns(newsDto);

            var request = new GetNewsByIdQuery(id);

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Image.Base64.Should().Be(fakeBase);
        }
    }
}
