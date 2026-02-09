using FluentAssertions;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class DeleteNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteNewsHandler _handler;

        public DeleteNewsHandlerTests()
        {
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new DeleteNewsHandler(
                _repositoryWrapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsNotFound()
        {
            // arrange
            int id = 1;
            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                null,
                false
            ))
                .ReturnsAsync((NewsEntity)null);
            var command = new DeleteNewsCommand(id);

            // act
            var result = await _handler.Handle(command, CancellationToken.None);


            // assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message
                .Contains($"No news found by entered Id - {id}"));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsNotDeleted()
        {
            // arrange
            int id = 1;

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                null,
                false
            ))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            var command = new DeleteNewsCommand(id);

            // act
            var result = await _handler.Handle(command, CancellationToken.None);

            // assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains("Failed to delete news"));
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenNewsSuccesfullyDeleted()
        {
            // arrange
            int id = 1;

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now
            };

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                null,
                false
            ))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new DeleteNewsCommand(id);

            // act
            var result = await _handler.Handle(command, CancellationToken.None);

            // assert
            _repositoryWrapperMock.Verify(
                r => r.NewsRepository.Delete(news),
                Times.Once);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenNewsSuccesfullyDeletedWithImage()
        {
            // arrange
            int id = 1;

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now,
                Image = new Image(),
            };

            _repositoryWrapperMock.Setup(r => r.ImageRepository.Delete(It.IsAny<Image>()));

            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                null,
                false
            ))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new DeleteNewsCommand(id);

            // act
            var result = await _handler.Handle(command, CancellationToken.None);

            // assert
            _repositoryWrapperMock.Verify(
                r => r.NewsRepository.Delete(news),
                Times.Once);

            _repositoryWrapperMock.Verify(
                r => r.ImageRepository.Delete(news.Image),
                Times.Once);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
