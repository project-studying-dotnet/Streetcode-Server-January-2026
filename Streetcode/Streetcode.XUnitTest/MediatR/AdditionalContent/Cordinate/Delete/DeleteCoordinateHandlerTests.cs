using System.Linq.Expressions;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Delete;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate.Delete;

public class DeleteCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
    private readonly DeleteCoordinateHandler handler;

    public DeleteCoordinateHandlerTests()
    {
        this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
        this.handler = new DeleteCoordinateHandler(this.mockRepoWrapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityExistsAndDeleted()
    {
        // Arrange
        var coordinate = new StreetcodeCoordinate { Id = 1 };
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinate);
        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(new DeleteCoordinateCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(new StreetcodeCoordinate());
        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(new DeleteCoordinateCommand(1), CancellationToken.None);

        // Assert
        result.Value.Should().BeOfType<Unit>();
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteOnce_WhenEntityExists()
    {
        // Arrange
        var coordinate = new StreetcodeCoordinate { Id = 1 };
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinate);
        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await this.handler.Handle(new DeleteCoordinateCommand(1), CancellationToken.None);

        // Assert
        this.mockRepoWrapper.Verify(r => r.StreetcodeCoordinateRepository.Delete(coordinate), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityNotFound()
    {
        // Arrange
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync((StreetcodeCoordinate?)null);

        // Act
        var result = await this.handler.Handle(new DeleteCoordinateCommand(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesAsyncReturnsZero()
    {
        // Arrange
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(new StreetcodeCoordinate());
        this.mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await this.handler.Handle(new DeleteCoordinateCommand(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenEntityNotFound()
    {
        // Arrange
        int id = 1;
        this.mockRepoWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync((StreetcodeCoordinate?)null);
        var expectedError = $"Cannot find a coordinate with corresponding categoryId: {id}";

        // Act
        var result = await this.handler.Handle(new DeleteCoordinateCommand(id), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }
}