using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate.Update;

public class UpdateCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
    private readonly Mock<IMapper> mockMapper;
    private readonly UpdateCoordinateHandler handler;

    public UpdateCoordinateHandlerTests()
    {
        this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
        this.mockMapper = new Mock<IMapper>();

        this.handler = new UpdateCoordinateHandler(
            this.mockRepoWrapper.Object,
            this.mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO { Id = 1 };
        var coordinate = new StreetcodeCoordinate { Id = 1 };

        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(coordinate);

        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

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
        var result = await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<Unit>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(new StreetcodeCoordinate());

        // Returns 1 to simulate one row affected in the database
        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        this.mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtosCorrectly()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO { Id = 1 };

        // Act
        await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        this.mockMapper.Verify(m => m.Map<StreetcodeCoordinate>(coordinateDto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns((StreetcodeCoordinate?)null);

        // Act
        var result = await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(new StreetcodeCoordinate());

        // Simulating that the database update failed/entity wasn't found to update
        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO();
        this.mockMapper.Setup(m => m.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns((StreetcodeCoordinate?)null);
        var expectedError = "Cannot convert null to streetcodeCoordinate";

        // Act
        var result = await this.handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}