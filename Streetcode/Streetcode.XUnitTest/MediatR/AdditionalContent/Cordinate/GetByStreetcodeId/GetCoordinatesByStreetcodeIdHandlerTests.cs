using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
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

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_StreetcodeExists_ReturnsCorrectDataAndType()
    {
        // Arrange
        int streetcodeId = 1;
        var query = new GetCoordinatesByStreetcodeIdQuery(streetcodeId);

        var coordinates = new List<StreetcodeCoordinate>
        {
            new() { Id = 1, StreetcodeId = streetcodeId, Latitude = 1.1, Longitude = 2.2 },
            new() { Id = 2, StreetcodeId = streetcodeId, Latitude = 3.3, Longitude = 4.4 }
        };

        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Streetcode, bool>>>(), null))
            .ReturnsAsync(new Streetcode { Id = streetcodeId });

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository
            .GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinates);

        var handler = new GetCoordinatesByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<List<StreetcodeCoordinateDTO>>();
        result.Value.Count().Should().Be(2);
        result.Value.First().Latitude.Should().Be(1.1);
    }

    [Fact]
    public async Task Handle_StreetcodeDoesNotExist_ReturnsFailureWithErrorMessage()
    {
        // Arrange
        int streetcodeId = 99;
        var query = new GetCoordinatesByStreetcodeIdQuery(streetcodeId);

        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Streetcode, bool>>>(), null))
            .ReturnsAsync((Streetcode?)null);

        var handler = new GetCoordinatesByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain($"Cannot find a coordinates by a streetcode id: {streetcodeId}");
    }

    [Fact]
    public async Task Handle_CoordinatesCollectionIsNull_ReturnsFailureAndLogsError()
    {
        // Arrange
        int streetcodeId = 1;
        var query = new GetCoordinatesByStreetcodeIdQuery(streetcodeId);

        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Streetcode, bool>>>(), null))
            .ReturnsAsync(new Streetcode { Id = streetcodeId });

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository
            .GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync((IEnumerable<StreetcodeCoordinate>?)null);

        var handler = new GetCoordinatesByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _mockLogger.Verify(x => x.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }
}