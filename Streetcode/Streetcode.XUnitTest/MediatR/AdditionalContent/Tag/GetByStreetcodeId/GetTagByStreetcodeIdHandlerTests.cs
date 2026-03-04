using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent; 
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
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

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TagProfile());
        });
        _mapper = config.CreateMapper();
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
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(tagIndexed);

        var handler = new GetTagsByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var resultList = result.Value.ToList();
        resultList[0].Title.Should().Be("First");
        resultList[1].Title.Should().Be("Second");
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNull_ReturnsFailureAndLogsError()
    {
        // Arrange
        int streetcodeId = 1;
        var query = new GetTagByStreetcodeIdQuery(streetcodeId);

        _mockRepo.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>?)null);

        var handler = new GetTagsByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(DAL.Entities.AdditionalContent.Tag),
            streetcodeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCorrectDtoType()
    {
        // Arrange
        var query = new GetTagByStreetcodeIdQuery(1);

        _mockRepo.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(new List<StreetcodeTagIndex>());

        var handler = new GetTagsByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeAssignableTo<IEnumerable<StreetcodeTagDTO>>();
    }
}