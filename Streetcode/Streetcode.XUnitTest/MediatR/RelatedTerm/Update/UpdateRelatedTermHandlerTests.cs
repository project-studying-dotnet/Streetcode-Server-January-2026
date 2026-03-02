using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Update;

public class UpdateRelatedTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly Mock<IRelatedTermRepository> relatedTermRepositoryMock;
    private readonly IMapper mapper;
    private readonly UpdateRelatedTermHandler handler;

    public UpdateRelatedTermHandlerTests()
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

        this.handler = new UpdateRelatedTermHandler(
            this.mapper,
            this.loggerMock.Object,
            this.repositoryWrapperMock.Object);
    }

    private void SetupSaveChangesMock(int result)
    {
        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Handle_ReturnsUpdatedRelatedTerm_WhenUpdateSuccessful()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = "test-updated",
        };

        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            Word = "test",
            TermId = 1,
        };

        var updatedRelatedTermDto = new RelatedTermDTO
        {
            Id = 1,
            Word = "test-updated",
            TermId = 1,
        };

        var command = new UpdateRelatedTermCommand(updateRelatedTermDTO);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(relatedTerm);

        this.relatedTermRepositoryMock
            .Setup(r => r
                .Update(It.IsAny<DAL.Entities.Streetcode.TextContent.RelatedTerm>()))
            .Returns((EntityEntry<DAL.Entities.Streetcode.TextContent.RelatedTerm>)null!);

        this.SetupSaveChangesMock(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(updatedRelatedTermDto);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenRelatedTermDoesNotExist()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = "test-updated",
        };

        var command = new UpdateRelatedTermCommand(updateRelatedTermDTO);

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
            Messages.Error_EntityWithIdNotFound.Format(
                nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm),
                updateRelatedTermDTO.Id)));
    }

    [Fact]
    public async Task ReturnsFailedResult_WhenSaveUnsuccessful()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = "test-updated",
        };

        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            Word = "test",
            TermId = 1,
        };

        var updatedRelatedTermDto = new RelatedTermDTO
        {
            Id = 1,
            Word = "test-updated",
            TermId = 1,
        };

        var command = new UpdateRelatedTermCommand(updateRelatedTermDTO);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(relatedTerm);

        this.relatedTermRepositoryMock
            .Setup(r => r
                .Update(It.IsAny<DAL.Entities.Streetcode.TextContent.RelatedTerm>()))
            .Returns((EntityEntry<DAL.Entities.Streetcode.TextContent.RelatedTerm>)null!);

        this.SetupSaveChangesMock(0);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(
            Messages.Error_FailedToUpdateEntity.Format(
                nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm))));
    }
}