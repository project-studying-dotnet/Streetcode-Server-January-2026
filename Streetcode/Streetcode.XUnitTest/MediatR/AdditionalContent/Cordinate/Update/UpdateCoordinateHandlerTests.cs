using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.BLL.Mapping.AdditionalContent.Coordinates;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate;

public class UpdateCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly IMapper _mapper;

    public UpdateCoordinateHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new StreetcodeCoordinateProfile());
        });

        _mapper = new Mapper(configuration);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessAndCallsUpdate()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO
        {
            Id = 1,
            Latitude = 50.5m,
            Longtitude = 30.5m 
        };
        var command = new UpdateCoordinateCommand(coordinateDto);

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.Update(It.IsAny<StreetcodeCoordinate>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateCoordinateHandler(_mockRepo.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _mockRepo.Verify(r => r.StreetcodeCoordinateRepository.Update(It.IsAny<StreetcodeCoordinate>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_MapperReturnsNull_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateCoordinateCommand(null!);
        var handler = new UpdateCoordinateHandler(_mockRepo.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Cannot convert null to streetcodeCoordinate");
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ReturnsFailureMessage()
    {
        // Arrange
        var coordinateDto = new StreetcodeCoordinateDTO { Id = 1 };
        var command = new UpdateCoordinateCommand(coordinateDto);

        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var handler = new UpdateCoordinateHandler(_mockRepo.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Failed to update a streetcodeCoordinate");
    }
}