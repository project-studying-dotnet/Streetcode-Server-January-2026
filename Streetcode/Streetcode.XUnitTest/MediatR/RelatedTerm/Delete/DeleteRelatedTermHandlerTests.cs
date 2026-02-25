using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.Resources;
using Xunit;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Delete;

public class DeleteRelatedTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly Mock<IRelatedTermRepository> relatedTermRepositoryMock;
    private readonly IMapper mapper;
    private readonly DeleteRelatedTermHandler handler;

    public DeleteRelatedTermHandlerTests()
    {
        this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        this.loggerMock = new Mock<ILoggerService>();
        this.relatedTermRepositoryMock = new Mock<IRelatedTermRepository>();

        this.repositoryWrapperMock
            .Setup(r => r.RelatedTermRepository)
            .Returns(this.relatedTermRepositoryMock.Object);

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new RelatedTermProfile());
        });
        this.mapper = new Mapper(configuration);

        this.handler = new DeleteRelatedTermHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsDeletedRelatedTerm_WhenRelatedTermExists()
    {
        // Arrange
        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            TermId = 1,
            Word = "test",
        };

        var relatedTermDto = new RelatedTermDTO
        {
            Id = 1,
            TermId = 1,
            Word = "test",
        };

        var command = new DeleteRelatedTermCommand("test", 1);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(relatedTerm);

        this.relatedTermRepositoryMock
            .Setup(r => r.Delete(It.IsAny<DAL.Entities.Streetcode.TextContent.RelatedTerm>()));

        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(relatedTermDto);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenRelatedTermDoesNotExist()
    {
        var command = new DeleteRelatedTermCommand("test", 1);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((DAL.Entities.Streetcode.TextContent.RelatedTerm)null!);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(
            Messages.Error_RelatedTermNotFound.Format(command.Word, command.TermId)));
    }

    [Fact]
    public async Task ReturnsFailedResult_WhenSaveUnsuccessful()
    {
        // Arrange
        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            TermId = 1,
            Word = "test",
        };

        var relatedTermDto = new RelatedTermDTO
        {
            Id = 1,
            TermId = 1,
            Word = "test",
        };

        var command = new DeleteRelatedTermCommand("test", 1);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(relatedTerm);

        this.relatedTermRepositoryMock
            .Setup(r => r.Delete(It.IsAny<DAL.Entities.Streetcode.TextContent.RelatedTerm>()));

        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(
            Messages.Error_FailedToDeleteEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm))));
    }
}