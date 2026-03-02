using System.Linq.Expressions;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Delete;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate;

public class DeleteCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;

    public DeleteCoordinateHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
    }

    [Fact]
    public async Task Handle_ExistingCoordinate_ReturnsSuccess()
    {
        // Arrange
        int testId = 1;
        var command = new DeleteCoordinateCommand(
            testId);
        var coordinate = new StreetcodeCoordinate
        {
            Id = testId
        };

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
            .ReturnsAsync(coordinate);

        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var handler = new DeleteCoordinateHandler(
            _mockRepo.Object);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepo.Verify(r => r.StreetcodeCoordinateRepository.Delete(
            coordinate), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingCoordinate_ReturnsFailureWithCorrectMessage()
    {
        // Arrange
        int testId = 99;
        var command = new DeleteCoordinateCommand(
            testId);

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
            null))
            .ReturnsAsync((StreetcodeCoordinate?)null);

        var handler = new DeleteCoordinateHandler(
            _mockRepo.Object);

        var expectedError = Messages.Error_EntityWithIdNotFound.Format(
            nameof(StreetcodeCoordinate),
            testId);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockRepo.Verify(r => r.StreetcodeCoordinateRepository.Delete(
            It.IsAny<StreetcodeCoordinate>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ReturnsFailureMessage()
    {
        // Arrange
        int testId = 1;
        var command = new DeleteCoordinateCommand(
            testId);
        var coordinate = new StreetcodeCoordinate
        {
            Id = testId
        };

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
            null))
            .ReturnsAsync(coordinate);

        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        var handler = new DeleteCoordinateHandler(
            _mockRepo.Object);


        string expectedError = Messages.Error_FailedToDeleteEntity.Format(
            nameof(StreetcodeCoordinate));

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);
    }
}