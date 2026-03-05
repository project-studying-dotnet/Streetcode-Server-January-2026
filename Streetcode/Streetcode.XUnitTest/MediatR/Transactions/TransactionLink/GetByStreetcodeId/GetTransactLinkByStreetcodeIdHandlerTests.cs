using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Transactions;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Transactions.TransactionLink;

public class GetTransactLinkByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly IMapper _mapper;

    public GetTransactLinkByStreetcodeIdHandlerTests()
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
        int streetcodeId = 5;
        var link = new Streetcode.DAL.Entities.Transactions.TransactionLink
        {
            Id = 1,
            StreetcodeId = streetcodeId,
            Url = "https://streetcode.com/donate"
        };
        var query = new GetTransactLinkByStreetcodeIdQuery(
            streetcodeId);

        _mockRepo.Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.Transactions.TransactionLink, bool>>>(),
            It.IsAny<Func<IQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink>, IIncludableQueryable<Streetcode.DAL.Entities.Transactions.TransactionLink, object>>>(),
            It.IsAny<bool>()))
            .ReturnsAsync(link);

        var handler = new GetTransactLinkByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.StreetcodeId.Should().Be(streetcodeId);
    }

    [Fact]
    public async Task Handle_LinkDoesNotExist_ReturnsFailureAndLogsError()
    {
        // Arrange
        int streetcodeId = 10;
        var query = new GetTransactLinkByStreetcodeIdQuery(
            streetcodeId);

        _mockRepo.Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Streetcode.DAL.Entities.Transactions.TransactionLink, bool>>>(),
            null,
            false))
            .ReturnsAsync((Streetcode.DAL.Entities.Transactions.TransactionLink?)null);

        var handler = new GetTransactLinkByStreetcodeIdHandler(
            _mockRepo.Object,
            _mapper,
            _mockLogger.Object);

        var expectedError = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(Streetcode.DAL.Entities.Transactions.TransactionLink),
            streetcodeId);

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
}