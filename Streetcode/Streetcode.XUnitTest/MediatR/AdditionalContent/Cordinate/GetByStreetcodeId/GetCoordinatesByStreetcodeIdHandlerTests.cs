using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent.Coordinates;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate;

public class GetCoordinatesByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetCoordinatesByStreetcodeIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new StreetcodeCoordinateProfile());
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_StreetcodeExists_ReturnsCorrectDataAndType()
    {
        // Arrange
        int streetcodeId = 1;
        var query = new GetCoordinatesByStreetcodeIdQuery(streetcodeId);

        var coordinates = new List<StreetcodeCoordinate>
        {
            new() { Id = 1, StreetcodeId = streetcodeId, Latitude = 1.1m, Longtitude = 2.2m },
            new() { Id = 2, StreetcodeId = streetcodeId, Latitude = 3.3m, Longtitude = 4.4m }
        };

        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository
            .GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeCoordinate>, IIncludableQueryable<StreetcodeCoordinate, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(coordinates);

        var handler = new GetCoordinatesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IEnumerable<StreetcodeCoordinateDTO>>();
        result.Value.Count().Should().Be(2);
        result.Value.First().Latitude.Should().Be(1.1m);
    }

    [Fact]
    public async Task Handle_StreetcodeDoesNotExist_ReturnsFailureWithErrorMessage()
    {
        // Arrange
        int streetcodeId = 99;
        var query = new GetCoordinatesByStreetcodeIdQuery(streetcodeId);

        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((StreetcodeContent?)null);

        var handler = new GetCoordinatesByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(StreetcodeCoordinate),
            streetcodeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockRepo.Verify(r => r.StreetcodeCoordinateRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(),
            null,
            false), Times.Never);
    }
}