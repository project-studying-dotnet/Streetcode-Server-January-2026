using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag;

public class GetTagByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetTagByStreetcodeIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        // Real Mapper Setup
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_TagsExistForStreetcode_ReturnsSuccessWithSortedData()
    {
        // Arrange
        int streetcodeId = 1;
        var query = new GetTagByStreetcodeIdQuery(streetcodeId);

        var tagIndexed = new List<StreetcodeTagIndex>
        {
            new() { StreetcodeId = streetcodeId, Index = 2, Tag = new DAL.Entities.AdditionalContent.Tag { Title = "Second" } },
            new() { StreetcodeId = streetcodeId, Index = 1, Tag = new DAL.Entities.AdditionalContent.Tag { Title = "First" } }
        };

        _mockRepo.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync(tagIndexed);

        var handler = new GetTagByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        // Verify sorting logic (OrderBy Index)
        result.Value.First().Title.Should().Be("First");
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNull_ReturnsFailureAndLogsError()
    {
        // Arrange
        int streetcodeId = 1;
        var query = new GetTagByStreetcodeIdQuery(streetcodeId);
        string expectedError = $"Cannot find any tag by the streetcode id: {streetcodeId}";

        _mockRepo.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>?)null);

        var handler = new GetTagByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(expectedError);
        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCorrectDtoType()
    {
        // Arrange
        var query = new GetTagByStreetcodeIdQuery(1);
        _mockRepo.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(), null))
            .ReturnsAsync(new List<StreetcodeTagIndex>());

        var handler = new GetTagByStreetcodeIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeAssignableTo<IEnumerable<StreetcodeTagDTO>>();
    }
}