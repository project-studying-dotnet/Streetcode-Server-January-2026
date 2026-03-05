using System.Linq.Expressions;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Delete;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnit.BLL.MediatR.AdditionalContent.Coordinate;

public class DeleteCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;

    public DeleteCoordinateHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
    }

    [Fact]
    public async Task Handle_CoordinateNotFound_ReturnsFailResult()
    {
        // Arrange
        int testId = 1;

        _mockRepositoryWrapper.Setup(repo => repo.StreetcodeCoordinateRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCoordinate>, IIncludableQueryable<StreetcodeCoordinate, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((StreetcodeCoordinate?)null);

        var handler = new DeleteCoordinateHandler(_mockRepositoryWrapper.Object);
        var command = new DeleteCoordinateCommand(testId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _mockRepositoryWrapper.Verify(r => r.StreetcodeCoordinateRepository.Delete(It.IsAny<StreetcodeCoordinate>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CoordinateExists_DeletesAndReturnsOkResult()
    {
        // Arrange
        int testId = 1;
        var coordinate = new StreetcodeCoordinate { Id = testId };

        _mockRepositoryWrapper.Setup(repo => repo.StreetcodeCoordinateRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCoordinate>, IIncludableQueryable<StreetcodeCoordinate, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(coordinate);

        _mockRepositoryWrapper.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        var handler = new DeleteCoordinateHandler(_mockRepositoryWrapper.Object);
        var command = new DeleteCoordinateCommand(testId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepositoryWrapper.Verify(r => r.StreetcodeCoordinateRepository.Delete(coordinate), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncFails_ReturnsFailResult()
    {
        // Arrange
        int testId = 1;
        var coordinate = new StreetcodeCoordinate { Id = testId };

        _mockRepositoryWrapper.Setup(repo => repo.StreetcodeCoordinateRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCoordinate>, IIncludableQueryable<StreetcodeCoordinate, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(coordinate);

        _mockRepositoryWrapper.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0);

        var handler = new DeleteCoordinateHandler(_mockRepositoryWrapper.Object);
        var command = new DeleteCoordinateCommand(testId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}