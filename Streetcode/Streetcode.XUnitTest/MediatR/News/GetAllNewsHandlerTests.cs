using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.Newss;
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
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly IMapper _mapper;
        private readonly GetAllNewsHandler _handler;

        public GetAllNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();

            var config = new MapperConfiguration(conf =>
            {
                conf.AddProfile(new NewsProfile());
                conf.AddProfile(new ImageProfile());
            });

            _mapper = config.CreateMapper();

            _handler = new GetAllNewsHandler(
                _repositoryWrapperMock.Object,
                _mapper,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoNewsInDb()
        {
            // arrange
            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false
            ))
            .ReturnsAsync((IEnumerable<NewsEntity>)null!);

            var request = new GetAllNewsQuery();

            // act
            var result = await _handler.Handle(request, CancellationToken.None);

            // assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message == "There are no news in the database");
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDto_WhenNewsFound()
        {
            // arrange
            var now = DateTime.Now;

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
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false
            ))
            .ReturnsAsync(news);

            var request = new GetAllNewsQuery();

            // act
            var result = await _handler.Handle(request, CancellationToken.None);

            // assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewsFoundWithImage()
        {
            // arrange
            var now = DateTime.Now;
            var fakeBase = "fabe_base_64";

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
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                false
            ))
            .ReturnsAsync(news);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(fakeBase);

            var request = new GetAllNewsQuery();

            // act
            var result = await _handler.Handle(request, CancellationToken.None);

            // assert
            result.IsSuccess.Should().BeTrue();
            result.Value.FirstOrDefault().Image.Base64.Should().Be(fakeBase);
        }
    }
}
