using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Transactions;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetById;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Transactions.TransactionLink;

public class GetTransactLinkByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetTransactLinkByIdHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TransactionLinkProfile());
        });
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_LinkExists_ReturnsSuccessWithMappedDTO()
    {
        // Arrange
        int testId = 1;
        var link = new Streetcode.DAL.Entities.Transactions.TransactionLink
        {
            Id = testId,
            Url = "https://streetcode.com/donate",
            StreetcodeId = 10
        };
        var query = new GetTransactLinkByIdQuery(testId);

        _mockRepo.Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.Transactions.TransactionLink, bool>>>(),
            It.IsAny<Func<IQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink>, IIncludableQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(link);

        var handler = new GetTransactLinkByIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TransactLinkDTO>();
        result.Value.Id.Should().Be(testId);
        result.Value.Url.Should().Be("https://streetcode.com/donate");
    }

    [Fact]
    public async Task Handle_LinkDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        int testId = 99;
        var query = new GetTransactLinkByIdQuery(testId);

        _mockRepo.Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.Transactions.TransactionLink, bool>>>(),
            null,
            false))
            .ReturnsAsync((Streetcode.DAL.Entities.Transactions.TransactionLink?)null);

        var handler = new GetTransactLinkByIdHandler(_mockRepo.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _mockLogger.Verify(x => x.LogError(query, It.Is<string>(s => s.Contains(testId.ToString()))), Times.Once);
    }
}