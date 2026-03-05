using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Transactions;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Transactions.TransactionLink;

public class GetAllTransactLinksHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetAllTransactLinksHandlerTests()
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
    public async Task Handle_LinksExist_ReturnsSuccessWithMappedDTOs()
    {
        // Arrange
        var links = new List<Streetcode.DAL.Entities.Transactions.TransactionLink>
        {
            new() { Id = 1, Url = "https://streetcode.com/1", StreetcodeId = 1 },
            new() { Id = 2, Url = "https://streetcode.com/2", StreetcodeId = 2 }
        };

        _mockRepo.Setup(r => r.TransactLinksRepository.GetAllAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.Transactions.TransactionLink, bool>>>(),
            It.IsAny<Func<IQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink>, IIncludableQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(links);

        var handler = new GetAllTransactLinksHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(
            new GetAllTransactLinksQuery(),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().Url.Should().Be("https://streetcode.com/1");
    }

    [Fact]
    public async Task Handle_NoLinksFound_ReturnsFailureAndLogsError()
    {
        // Arrange
        var query = new GetAllTransactLinksQuery();

        _mockRepo.Setup(r => r.TransactLinksRepository.GetAllAsync(
            (Expression<Func<Streetcode.DAL.Entities.Transactions.TransactionLink, bool>>)null!,
            (Func<IQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink>, IIncludableQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink, object>>)null!,
            false))
            .ReturnsAsync(new List<Streetcode.DAL.Entities.Transactions.TransactionLink>());

        var handler = new GetAllTransactLinksHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntitiesNotFound.Format(
            nameof(Streetcode.DAL.Entities.Transactions.TransactionLink));

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);

        _mockLogger.Verify(x => x.LogError(
            query,
            expectedError), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsNull_ReturnsFailure()
    {
        // Arrange
        _mockRepo.Setup(r => r.TransactLinksRepository.GetAllAsync(
            null,
            null,
            false))
            .ReturnsAsync((IEnumerable<Streetcode.DAL.Entities.Transactions.TransactionLink>?)null);

        var handler = new GetAllTransactLinksHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var query = new GetAllTransactLinksQuery();

        string expectedError = Messages.Error_EntitiesNotFound.Format(
            nameof(Streetcode.DAL.Entities.Transactions.TransactionLink));

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedError);
    }
}