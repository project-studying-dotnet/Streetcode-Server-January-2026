using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate.Create;

public class CreateCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
    private readonly Mock<IMapper> mockMapper;
    private readonly CreateCoordinateHandler handler;

    public CreateCoordinateHandlerTests()
    {
        this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
        this.mockMapper = new Mock<IMapper>();

        this.handler = new CreateCoordinateHandler(
            this.mockRepoWrapper.Object,
            this.mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCoordinateIsCreated()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        var coordinate = new StreetcodeCoordinate();

        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(coordinate);

        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(new StreetcodeCoordinate());

        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<Unit>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems_MeaningOneRecordCreated()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(new StreetcodeCoordinate());

        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        this.mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapDtoToEntityCorrectly()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO { Latitude = 10, Longtitude = 20 };
        
        // Act
        await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        this.mockMapper.Verify(m => m.Map<StreetcodeCoordinate>(coordinateDto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMappedEntityIsNull()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns((StreetcodeCoordinate?)null);

        // Act
        var result = await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesAsyncReturnsZero()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(new StreetcodeCoordinate());

        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenCreationFails()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(new StreetcodeCoordinate());

        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);
        
        var expectedError = "Failed to create a streetcodeCoordinate";

        // Act
        var result = await this.handler.Handle(new CreateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}