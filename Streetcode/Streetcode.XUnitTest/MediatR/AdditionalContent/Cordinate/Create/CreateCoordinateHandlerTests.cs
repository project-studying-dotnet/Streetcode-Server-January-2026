using AutoMapper;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate;

public class CreateCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly IMapper _mapper;

    public CreateCoordinateHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();

        // Real Mapper Setup
        var myProfile = new MappingProfile(); // Replace with your actual BLL Mapping Profile class
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
        _mapper = new Mapper(configuration);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessAndCallsCreate()
    {
        // Arrange
        var command = new CreateCoordinateCommand(new StreetcodeCoordinateDTO { Latitude = 10, Longitude = 20 });

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.Create(It.IsAny<StreetcodeCoordinate>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateCoordinateHandler(_mockRepo.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _mockRepo.Verify(r => r.StreetcodeCoordinateRepository.Create(It.IsAny<StreetcodeCoordinate>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MapperReturnsNull_ReturnsFailure()
    {
        // Arrange
        // Passing a null DTO to force the mapper/handler logic to trigger the null check
        var command = new CreateCoordinateCommand(null!);
        var handler = new CreateCoordinateHandler(_mockRepo.Object, _mapper);

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
        var command = new CreateCoordinateCommand(new StreetcodeCoordinateDTO());

        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // Simulate database error

        var handler = new CreateCoordinateHandler(_mockRepo.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Failed to create a streetcodeCoordinate");
    }
}