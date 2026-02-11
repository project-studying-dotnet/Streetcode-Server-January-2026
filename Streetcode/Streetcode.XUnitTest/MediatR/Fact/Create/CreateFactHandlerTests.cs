using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Fact.Create
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Repositories.Interfaces;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.TextContent;
    using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
    using Streetcode.DAL.Entities.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Media.Images;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;
    using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
    using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;

    public class CreateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IFactRepository> factRepositoryMock;
        private readonly Mock<IImageRepository> imageRepositoryMock;
        private readonly Mock<IImageDetailsRepository> imageDetailsRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly CreateFactHandler handler;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };

        public CreateFactHandlerTests()
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

            this.handler = new CreateFactHandler(
                this.mapper,
                this.repositoryWrapperMock.Object,
                this.loggerMock.Object);
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

        private void SetupCreateFact(FactEntity fact)
        {
            this.factRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<FactEntity>()))
                .ReturnsAsync(fact);
        }

        private void SetupSaveChanges()
        {
            this.repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        }

        private static CreateFactDTO GetCreateFactDTO()
        {
            return new CreateFactDTO
            {
                Title = "New Fact",
                FactContent = "Content",
                ImageId = 1,
                StreetcodeId = 1,
                ImageDescription = "New Description",
            };
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRequestIsValid()
        {
            // Arrange
            var command = new CreateFactCommand(GetCreateFactDTO());
            var image = new ImageEntity { Id = 1, MimeType = "image/jpeg" };
            var createdFact = new FactEntity { Id = 10, Title = "New Fact" };

            this.SetupImage(image);
            this.SetupImageDetails(null);
            this.SetupCreateFact(createdFact);
            this.SetupSaveChanges();

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            result.Value.Title.Should().Be("New Fact");
            this.factRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FactEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreatesImageDetails_WhenTheyDoNotExistInImage()
        {
            // Arrange
            var dto = GetCreateFactDTO();
            var command = new CreateFactCommand(dto);

            var streetcode = new StreetcodeEntity { Id = 1 };
            var image = new ImageEntity { Id = 1, MimeType = "image/jpeg" };
            var createdFact = new FactEntity { Id = 10 };

            this.SetupImage(image);
            this.SetupImageDetails(null);
            this.SetupCreateFact(createdFact);
            this.SetupSaveChanges();

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            this.imageDetailsRepositoryMock.Verify(
                x => x.CreateAsync(It.Is<ImageDetails>(d => d.Title == "New Description")), Times.Once);
            this.imageDetailsRepositoryMock.Verify(x => x.Update(It.IsAny<ImageDetails>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdatesImageDetails_WhenImageDetailsExist()
        {
            // Arrange
            var dto = GetCreateFactDTO();
            dto.ImageDescription = "Updated Description";
            var command = new CreateFactCommand(dto);

            var streetcode = new StreetcodeEntity { Id = 1 };
            var image = new ImageEntity { Id = 1, MimeType = "image/png" };
            var existingDetails = new ImageDetails { Id = 5, ImageId = 1, Title = "Old Description" };
            var createdFact = new FactEntity { Id = 10 };

            this.SetupImage(image);
            this.SetupImageDetails(existingDetails);
            this.SetupCreateFact(createdFact);
            this.SetupSaveChanges();

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            this.imageDetailsRepositoryMock.Verify(
                x => x.Update(It.Is<ImageDetails>(d => d.Title == "Updated Description")), Times.Once);
            this.imageDetailsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ImageDetails>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenImageNotFound()
        {
            // Arrange
            var command = new CreateFactCommand(GetCreateFactDTO());
            this.SetupImage(null);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(Messages.Error_EntityWithIdNotFound.Format(nameof(Image), command.Fact.ImageId), result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenImageHasInvalidMimeType()
        {
            // Arrange
            var command = new CreateFactCommand(GetCreateFactDTO());
            var image = new ImageEntity { Id = 1, MimeType = "image/gif" };
            var allowedTypes = string.Join(",", AllowedImageTypes);
            var errorMsg = Messages.Error_InvalidImageFormat.Format(image.MimeType, allowedTypes);
            this.SetupImage(image);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(errorMsg, result.Errors.First().Message);

            this.factRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FactEntity>()), Times.Never);
        }
    }
}