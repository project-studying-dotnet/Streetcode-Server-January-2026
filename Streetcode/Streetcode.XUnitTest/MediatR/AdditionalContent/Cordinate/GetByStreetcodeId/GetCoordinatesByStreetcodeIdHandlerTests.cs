using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;

public class GetCoordinatesByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<ILoggerService> mockLogger;
    private readonly GetCoordinatesByStreetcodeIdHandler handler;

    public GetCoordinatesByStreetcodeIdHandlerTests()
    {
        this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
        this.mockMapper = new Mock<IMapper>();
        this.mockLogger = new Mock<ILoggerService>();

        this.handler = new GetCoordinatesByStreetcodeIdHandler(
            this.mockRepoWrapper.Object,
            this.mockMapper.Object,
            this.mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        SetupStreetcodeExists(1);
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(new List<StreetcodeCoordinate>());

        // Act
        var result = await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        SetupStreetcodeExists(1);
        var coordinates = new List<StreetcodeCoordinate>();
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinates);
        this.mockMapper.Setup(m => m.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates))
            .Returns(new List<StreetcodeCoordinateDTO>());

        // Act
        var result = await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().BeAssignableTo<IEnumerable<StreetcodeCoordinateDTO>>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems()
    {
        // Arrange
        SetupStreetcodeExists(1);
        var coordinates = new List<StreetcodeCoordinate> { new(), new() };
        var coordinateDtos = new List<StreetcodeCoordinateDTO> { new(), new() };
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinates);
        this.mockMapper.Setup(m => m.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates))
            .Returns(coordinateDtos);

        // Act
        var result = await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtosCorrectly()
    {
        // Arrange
        SetupStreetcodeExists(1);
        var coordinates = new List<StreetcodeCoordinate> { new() { Id = 1 } };
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinates);

        // Act
        await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        this.mockMapper.Verify(m => m.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        SetupStreetcodeExists(1);
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync((IEnumerable<StreetcodeCoordinate>?)null);

        // Act
        var result = await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        this.mockRepoWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent?)null);

        // Act
        var result = await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage()
    {
        // Arrange
        int id = 1;
        this.mockRepoWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent?)null);
        var errorMsg = $"Cannot find a coordinates by a streetcode id: {id}, because such streetcode doesn`t exist";

        // Act
        var result = await this.handler.Handle(new GetCoordinatesByStreetcodeIdQuery(id), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
    }

    private void SetupStreetcodeExists(int id)
    {
        this.mockRepoWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(new StreetcodeContent { Id = id });
    }
}