using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Fact.Update
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Repositories.Interfaces;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.TextContent;
    using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;
    using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;

    public class UpdateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IFactRepository> factRepositoryMock;
        private readonly Mock<IImageRepository> imageRepositoryMock;
        private readonly Mock<IImageDetailsRepository> imageDetailsRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly UpdateFactHandler handler;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };

        public UpdateFactHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.factRepositoryMock = new Mock<IFactRepository>();
            this.imageRepositoryMock = new Mock<IImageRepository>();
            this.imageDetailsRepositoryMock = new Mock<IImageDetailsRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock.Setup(r => r.FactRepository)
                .Returns(this.factRepositoryMock.Object);
            this.repositoryWrapperMock.Setup(r => r.ImageRepository)
                .Returns(this.imageRepositoryMock.Object);
            this.repositoryWrapperMock.Setup(r => r.ImageDetailsRepository)
                .Returns(this.imageDetailsRepositoryMock.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new FactProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new UpdateFactHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        private void SetupFact(FactEntity? fact)
        {
            this.factRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(fact);
        }

        private void SetupImage(ImageEntity? image)
        {
            this.imageRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(image);
        }

        private void SetupImageDetails(ImageDetails? details)
        {
            this.imageDetailsRepositoryMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ImageDetails, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(details);
        }

        private void SetupSaveChanges(int result)
        {
            this.repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(result);
        }

        private static UpdateFactDTO GetUpdateFactDTO(int id)
        {
            return new UpdateFactDTO
            {
                Id = id,
                Title = "Updated Title",
                FactContent = "Updated Content",
                ImageId = 1,
                ImageDescription = "Updated Description",
            };
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRequestIsValid()
        {
            // Arrange
            int testId = 1;
            var command = new UpdateFactCommand(GetUpdateFactDTO(testId));

            var existingFact = new FactEntity { Id = testId, Title = "Old Title" };
            var existingImage = new ImageEntity { Id = 1, MimeType = "image/jpeg" };
            var existingDetails = new ImageDetails { Id = 5, ImageId = 1, Title = "Old Description" };

            this.SetupFact(existingFact);
            this.SetupImage(existingImage);
            this.SetupImageDetails(existingDetails);
            this.SetupSaveChanges(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            result.Value.Title.Should().Be("Updated Title");

            this.factRepositoryMock.Verify(
                x => x.Update(It.Is<FactEntity>(f => f.Title == "Updated Title")), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdatesImageDetails_WhenTheyExist()
        {
            // Arrange
            int testId = 1;
            var dto = GetUpdateFactDTO(testId);
            var command = new UpdateFactCommand(dto);

            var existingFact = new FactEntity { Id = testId };
            var existingImage = new ImageEntity { Id = 1, MimeType = "image/jpeg" };
            var existingDetails = new ImageDetails { Id = 5, ImageId = 1, Title = "Old Description" };

            this.SetupFact(existingFact);
            this.SetupImage(existingImage);
            this.SetupImageDetails(existingDetails);
            this.SetupSaveChanges(1);

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            this.imageDetailsRepositoryMock.Verify(
                x => x.Update(It.Is<ImageDetails>(d => d.Title == "Updated Description")), Times.Once);
            this.imageDetailsRepositoryMock.Verify(
                x => x.CreateAsync(It.IsAny<ImageDetails>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CreatesImageDetails_WhenTheyDoNotExist()
        {
            // Arrange
            int testId = 1;
            var dto = GetUpdateFactDTO(testId);
            var command = new UpdateFactCommand(dto);

            var existingFact = new FactEntity { Id = testId };
            var existingImage = new ImageEntity { Id = 1, MimeType = "image/png" };

            this.SetupFact(existingFact);
            this.SetupImage(existingImage);
            this.SetupImageDetails(null);
            this.SetupSaveChanges(1);

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            this.imageDetailsRepositoryMock.Verify(
                x => x.CreateAsync(It.Is<ImageDetails>(d => d.Title == "Updated Description")), Times.Once);
            this.imageDetailsRepositoryMock.Verify(
                x => x.Update(It.IsAny<ImageDetails>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenFactNotFound()
        {
            // Arrange
            int testId = 1;
            var command = new UpdateFactCommand(GetUpdateFactDTO(testId));
            string expectedError = Messages.Error_EntityWithIdNotFound.Format(
                nameof(DAL.Entities.Streetcode.TextContent.Fact),
                testId);

            this.SetupFact(null);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedError, result.Errors.First().Message);
            this.loggerMock.Verify(x => x.LogError(It.IsAny<object>(), expectedError), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenImageNotFound()
        {
            // Arrange
            int testId = 1;
            var command = new UpdateFactCommand(GetUpdateFactDTO(testId));
            var existingFact = new FactEntity { Id = testId };
            string expectedError = Messages.Error_EntityWithIdNotFound.Format(
                nameof(Image),
                testId);

            this.SetupFact(existingFact);
            this.SetupImage(null);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedError, result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenImageHasInvalidMimeType()
        {
            // Arrange
            int testId = 1;
            var command = new UpdateFactCommand(GetUpdateFactDTO(testId));
            var existingFact = new FactEntity { Id = testId };
            var invalidImage = new ImageEntity { Id = 1, MimeType = "image/gif" };
            var allowedTypes = string.Join(",", AllowedImageTypes);
            var errorMsg = Messages.Error_InvalidImageFormat.Format(invalidImage.MimeType, allowedTypes);

            this.SetupFact(existingFact);
            this.SetupImage(invalidImage);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors.First().Message);

            this.factRepositoryMock.Verify(x => x.Update(It.IsAny<FactEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            int testId = 1;
            var command = new UpdateFactCommand(GetUpdateFactDTO(testId));
            var existingFact = new FactEntity { Id = testId };
            var existingImage = new ImageEntity { Id = 1, MimeType = "image/jpeg" };
            string expectedError = Messages.Error_FailedToUpdateEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.Fact));

            this.SetupFact(existingFact);
            this.SetupImage(existingImage);
            this.SetupImageDetails(null);
            this.SetupSaveChanges(0);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedError, result.Errors.First().Message);
            this.loggerMock.Verify(x => x.LogError(It.IsAny<object>(), expectedError), Times.Once);
        }
    }
}