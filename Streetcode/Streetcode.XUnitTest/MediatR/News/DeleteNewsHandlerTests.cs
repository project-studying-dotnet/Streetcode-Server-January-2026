using FluentAssertions;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.Delete;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
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
            // Arrange
            int id = 1;
            _repositoryWrapperMock.Setup(r => r.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                null,
                false
            ))
                .ReturnsAsync((NewsEntity)null);
            var command = new DeleteNewsCommand(id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(Messages.Error_EntityWithIdNotFound.Format(
                nameof(DAL.Entities.News.News),
                id));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewsNotDeleted()
        {
            // Arrange
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

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(Messages.Error_FailedToDeleteEntity.Format(
                nameof(DAL.Entities.News.News)));
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenNewsSuccesfullyDeleted()
        {
            // Arrange
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

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repositoryWrapperMock.Verify(
                r => r.NewsRepository.Delete(news),
                Times.Once);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenNewsSuccesfullyDeletedWithImage()
        {
            // Arrange
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

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
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
