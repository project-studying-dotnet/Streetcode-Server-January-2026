using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;
using Streetcode.BLL.Mapping.AdditionalContent.Coordinates;
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


        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new StreetcodeCoordinateProfile());
        });
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
            new() { Id = 1, StreetcodeId = streetcodeId, Latitude = 1.1m, Longtitude = 2.2m },
            new() { Id = 2, StreetcodeId = streetcodeId, Latitude = 3.3m, Longtitude = 4.4m }
        };

        // FIXED: Using StreetcodeContent instead of Streetcode
        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository
            .GetAllAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinates);

        var handler = new GetCoordinatesByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

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

        // FIXED: Using StreetcodeContent instead of Streetcode
        _mockRepo.Setup(r => r.StreetcodeRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent?)null);

        var handler = new GetCoordinatesByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain($"{streetcodeId}");
    }
}