using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag.GetByStreetcodeId;

public class GetTagByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTagByStreetcodeIdHandler _handler;

    public GetTagByStreetcodeIdHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new GetTagByStreetcodeIdHandler(
            _mockRepoWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var tagIndexes = new List<StreetcodeTagIndex> { new() { StreetcodeId = 1 } };
        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync(tagIndexes);

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDataType()
    {
        // Arrange
        var tagIndexes = new List<StreetcodeTagIndex>();
        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync(tagIndexes);

        _mockMapper.Setup(m => m.Map<IEnumerable<StreetcodeTagDTO>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
            .Returns(new List<StreetcodeTagDTO>());

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().BeAssignableTo<IEnumerable<StreetcodeTagDTO>>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCountOfItems()
    {
        // Arrange
        var tagIndexes = new List<StreetcodeTagIndex> { new(), new() };
        var tagDtos = new List<StreetcodeTagDTO> { new(), new() };

        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            null))
            .ReturnsAsync(tagIndexes);

        _mockMapper.Setup(m => m.Map<IEnumerable<StreetcodeTagDTO>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
            .Returns(tagDtos);

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderedItemsByIndex()
    {
        // Arrange
        var tagIndexes = new List<StreetcodeTagIndex>
        {
            new() { Index = 2 },
            new() { Index = 1 }
        };

        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync(tagIndexes);

        // Act
        await _handler.Handle(new GetTagByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        // Verify that the mapper receives the list ordered by Index (1 then 2)
        _mockMapper.Verify(m => m.Map<IEnumerable<StreetcodeTagDTO>>(
            It.Is<IEnumerable<StreetcodeTagIndex>>(list =>
                list.First().Index == 1 && list.Last().Index == 2)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDataIsNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            null))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>?)null);

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectErrorMessage_WhenTagsNotFound()
    {
        // Arrange
        int streetcodeId = 1;
        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            null))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>?)null);

        var expectedError = $"Cannot find any tag by the streetcode id: {streetcodeId}";

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedError);
    }

    [Fact]
    public async Task Handle_ShouldCallLogger_WhenDataIsNull()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            null))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>?)null);

        // Act
        await _handler.Handle(new GetTagByStreetcodeIdQuery(1), CancellationToken.None);

        // Assert
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetTagByStreetcodeIdQuery>(), It.IsAny<string>()), Times.Once);
    }
}