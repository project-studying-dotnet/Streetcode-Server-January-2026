using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
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
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly SortedByDateTimeHandler _handler;

        public SortedByDateTimeHanlderTests()
        {
            _blobServiceMock = new Mock<IBlobService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new SortedByDateTimeHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoNewsInDb()
        {
            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync((IEnumerable<NewsEntity>)null!);

            var request = new SortedByDateTimeQuery();

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(e => e.Message == "There are no news in the database");
        }

        [Fact]
        public async Task Handle_ShouldReturnSortedNewsDtosWithImage_WhenNewsExists()
        {
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

            var allNewsDto = new List<NewsDTO>
            {
                new NewsDTO { Id = 1, URL = "url1", Image = new ImageDTO(), CreationDate = midDate },
                new NewsDTO { Id = 2, URL = "url2", Image = new ImageDTO(), CreationDate = oldDate },
                new NewsDTO { Id = 3, URL = "url3", Image = new ImageDTO(), CreationDate = newDate },
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<IEnumerable<NewsDTO>> (It.IsAny<IEnumerable<NewsEntity>>()))
                .Returns(allNewsDto);

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(expectedBase64);

            var request = new SortedByDateTimeQuery();

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.FirstOrDefault().Image.Base64.Should().Be(expectedBase64);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Exactly(allNewsDto.Count));
            res.Value.Should().BeInDescendingOrder(x => x.CreationDate);
        }

        [Fact]
        public async Task Handle_ShouldReturnSortedNewsDtosWithOutImage_WhenNewsExists()
        {
            var oldDate = DateTime.Now.AddYears(-2);
            var midDate = DateTime.Now.AddYears(-1);
            var newDate = DateTime.Now;

            var allNews = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, URL = "url1", CreationDate = midDate },
                new NewsEntity { Id = 2, URL = "url2", CreationDate = oldDate },
                new NewsEntity { Id = 3, URL = "url3", CreationDate = newDate },
            };

            var allNewsDto = new List<NewsDTO>
            {
                new NewsDTO { Id = 1, URL = "url1", CreationDate = midDate },
                new NewsDTO { Id = 2, URL = "url2", CreationDate = oldDate },
                new NewsDTO { Id = 3, URL = "url3", CreationDate = newDate },
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetAllAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()
            ))
            .ReturnsAsync(allNews);

            _mapperMock.Setup(m => m.Map<IEnumerable<NewsDTO>>(It.IsAny<IEnumerable<NewsEntity>>()))
                .Returns(allNewsDto);

            var request = new SortedByDateTimeQuery();

            var res = await _handler.Handle(request, CancellationToken.None);

            res.IsSuccess.Should().BeTrue();
            res.Value.Should().BeEquivalentTo(allNewsDto);
            _blobServiceMock.Verify(
                bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()),
                Times.Never);
            res.Value.Should().BeInDescendingOrder(x => x.CreationDate);
        }
    }
}
