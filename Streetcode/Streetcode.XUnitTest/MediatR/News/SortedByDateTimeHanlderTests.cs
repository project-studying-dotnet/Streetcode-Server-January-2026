using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.Newss;
using Streetcode.BLL.MediatR.Newss.SortedByDateTime;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class SortedByDateTimeHanlderTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly IMapper _mapper;
        private readonly SortedByDateTimeHandler _handler;

        public SortedByDateTimeHanlderTests()
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

            _handler = new SortedByDateTimeHandler(
                _repositoryWrapperMock.Object,
                _mapper,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoNewsInDb()
        {
            // arrange
            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync((IEnumerable<NewsEntity>)null!);

            var request = new SortedByDateTimeQuery();

            // act
            var res = await _handler.Handle(request, CancellationToken.None);

            // assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(e => e.Message == "There are no news in the database");
        }

        [Fact]
        public async Task Handle_ShouldReturnSortedNewsDtosWithImage_WhenNewsExists()
        {
            // arrange
            var oldDate = DateTime.Now.AddYears(-2);
            var midDate = DateTime.Now.AddYears(-1);
            var newDate = DateTime.Now;

            var expectedBase64 = "fake_base64_string";

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, URL = "url1", Image = new Image(), CreationDate = midDate },
                new NewsEntity { Id = 2, URL = "url2", Image = new Image(), CreationDate = oldDate },
                new NewsEntity { Id = 3, URL = "url3", Image = new Image(), CreationDate = newDate },
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(allNews);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(expectedBase64);

            var request = new SortedByDateTimeQuery();

            // act
            var res = await _handler.Handle(request, CancellationToken.None);

            // assert
            res.IsSuccess.Should().BeTrue();
            res.Value.FirstOrDefault().Image.Base64.Should().Be(expectedBase64);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Exactly(allNews.Count));
            res.Value.Should().BeInDescendingOrder(x => x.CreationDate);
        }

        [Fact]
        public async Task Handle_ShouldReturnSortedNewsDtosWithOutImage_WhenNewsExists()
        {
            // arrange
            var oldDate = DateTime.Now.AddYears(-2);
            var midDate = DateTime.Now.AddYears(-1);
            var newDate = DateTime.Now;

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, URL = "url1", CreationDate = midDate },
                new NewsEntity { Id = 2, URL = "url2", CreationDate = oldDate },
                new NewsEntity { Id = 3, URL = "url3", CreationDate = newDate },
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(allNews);

            var request = new SortedByDateTimeQuery();

            // act
            var res = await _handler.Handle(request, CancellationToken.None);

            // assert
            res.IsSuccess.Should().BeTrue();
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Never);
            res.Value.Should().BeInDescendingOrder(x => x.CreationDate);
        }
    }
}
