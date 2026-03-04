using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.AdditionalContent;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.AdditionalContent.Tag;

public class GetTagByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetTagByIdHandlerTests()
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
    public async Task Handle_TagExists_ReturnsSuccessWithMappedTag()
    {
        // Arrange
        int testId = 5;
        var tagEntity = new DAL.Entities.AdditionalContent.Tag
        {
            Id = testId,
            Title = "Culture"
        };
        var query = new GetTagByIdQuery(testId);

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(tagEntity);

        var handler = new GetTagByIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TagDTO>();
        result.Value.Id.Should().Be(testId);
        result.Value.Title.Should().Be("Culture");
    }

    [Fact]
    public async Task Handle_TagDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        int testId = 999;
        var query = new GetTagByIdQuery(testId);

        _mockRepo.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.AdditionalContent.Tag>, IIncludableQueryable<DAL.Entities.AdditionalContent.Tag, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag?)null);

        var handler = new GetTagByIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntityWithIdNotFound.Format(
            nameof(DAL.Entities.AdditionalContent.Tag),
            testId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockLogger.Verify(x => x.LogError(query, expectedError), Times.Once);
    }
}