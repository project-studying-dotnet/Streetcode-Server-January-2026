using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class GetAllNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllNewsHandler _handler;

        public GetAllNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllNewsHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoNewsInDb()
        {
            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync((IEnumerable<NewsEntity>)null!);

            var request = new GetAllNewsQuery();

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message == "There are no news in the database");
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDto_WhenNewsFound()
        {
            var now = DateTime.Now;

            var newsDto = new List<NewsDTO>
            {
                new NewsDTO
                {
                    Title = "Test Title",
                    Text = "Sample text",
                    URL = "https://github.com/",
                    CreationDate = now,
                },
            };

            var news = new List<NewsEntity>
            {
                new NewsEntity
                {
                    Title = "Test Title",
                    Text = "Sample text",
                    URL = "https://github.com/",
                    CreationDate = now,
                },
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(news);

            _mapperMock.Setup(m => m.Map<IEnumerable<NewsDTO>>(It.IsAny<IEnumerable<NewsEntity>>()))
                .Returns(newsDto);

            var request = new GetAllNewsQuery();

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(newsDto);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewsFoundWithImage()
        {
            var now = DateTime.Now;
            var fakeBase = "fabe_base_64";

            var newsDto = new List<NewsDTO>
            {
                new NewsDTO
                {
                    Title = "Test Title",
                    Text = "Sample text",
                    URL = "https://github.com/",
                    CreationDate = now,
                    Image = new ImageDTO(),
                },
            };

            var news = new List<NewsEntity>
            {
                new NewsEntity
                {
                    Title = "Test Title",
                    Text = "Sample text",
                    URL = "https://github.com/",
                    CreationDate = now,
                    Image = new Image(),

                },
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(news);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(fakeBase);

            _mapperMock.Setup(m => m.Map<IEnumerable<NewsDTO>>(It.IsAny<IEnumerable<NewsEntity>>()))
                .Returns(newsDto);

            var request = new GetAllNewsQuery();

            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.FirstOrDefault().Image.Base64.Should().Be(fakeBase);
        }
    }
}
