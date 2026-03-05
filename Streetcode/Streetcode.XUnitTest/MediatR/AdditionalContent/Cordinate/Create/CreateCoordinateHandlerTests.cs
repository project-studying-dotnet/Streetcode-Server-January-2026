using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.BLL.Mapping.AdditionalContent.Coordinates;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Coordinate;

public class CreateCoordinateHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly IMapper _mapper;

    public CreateCoordinateHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new StreetcodeCoordinateProfile());
            // Ensure mapping from DTO to Entity is available if not in profile
            cfg.CreateMap<StreetcodeCoordinateDTO, StreetcodeCoordinate>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessAndCallsCreate()
    {
        // Arrange
        var command = new CreateCoordinateCommand(
            new StreetcodeCoordinateDTO
            {
                Latitude = 10,
                Longtitude = 20
            });

        // FIXED: Changed Create to CreateAsync to match the Handler implementation
        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.CreateAsync(
            It.IsAny<StreetcodeCoordinate>()))
            .ReturnsAsync(new StreetcodeCoordinate());

        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var handler = new CreateCoordinateHandler(
            _mockRepo.Object,
            _mapper);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        // FIXED: Verifying CreateAsync because that is what the Handler actually invokes
        _mockRepo.Verify(r => r.StreetcodeCoordinateRepository.CreateAsync(
            It.IsAny<StreetcodeCoordinate>()), Times.Once);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_MapperReturnsNull_ReturnsFailure()
    {
        // Arrange
        // Passing null ensures the mapper returns null or throws, 
        // triggering the safety check in the Handler.
        var command = new CreateCoordinateCommand(null!);

        var handler = new CreateCoordinateHandler(
            _mockRepo.Object,
            _mapper);

        var expectedError = Messages.Error_ConvertNullToEntity.Format(
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

    [Fact]
    public async Task Handle_SaveFails_ReturnsFailure()
    {
        // Arrange
        var command = new CreateCoordinateCommand(new StreetcodeCoordinateDTO());

        _mockRepo.Setup(r => r.StreetcodeCoordinateRepository.CreateAsync(It.IsAny<StreetcodeCoordinate>()))
            .ReturnsAsync(new StreetcodeCoordinate());

        // Simulate database save failure (returning 0 rows affected)
        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        var handler = new CreateCoordinateHandler(_mockRepo.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}