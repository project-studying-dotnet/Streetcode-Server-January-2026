using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Create;

public class CreateRelatedTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly Mock<IRelatedTermRepository> relatedTermRepositoryMock;
    private readonly IMapper mapper;
    private readonly CreateRelatedTermHandler handler;

    public CreateRelatedTermHandlerTests()
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

        this.handler = new CreateRelatedTermHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    private void SetupSaveChangesMock(int result)
    {
        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Handle_ReturnsCreatedRelatedTerm_WhenCreateSuccessful()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = "test",
            TermId = 1,
        };

        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            Word = "test",
            TermId = 1,
        };

        var createdRelatedTermDto = new RelatedTermDTO
        {
            Id = 1,
            Word = "test",
            TermId = 1,
        };

        var command = new CreateRelatedTermCommand(createRelatedTermDTO);

        this.relatedTermRepositoryMock
            .Setup(r => r
                .CreateAsync(It.IsAny<DAL.Entities.Streetcode.TextContent.RelatedTerm>()))
            .ReturnsAsync(relatedTerm);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<
                    Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync([]);

        this.SetupSaveChangesMock(1);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(createdRelatedTermDto);
    }

    [Fact]
    public async Task ReturnsFailedResult_WhenTermAlreadyExists()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = "test",
            TermId = 1,
        };

        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            Word = "test",
            TermId = 1,
        };

        var command = new CreateRelatedTermCommand(createRelatedTermDTO);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<
                    Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                        IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync([relatedTerm]);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(Messages.Error_WordDefinitionExists));
    }

    [Fact]
    public async Task ReturnsFailedResult_WhenSaveUnsuccessful()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = "test",
            TermId = 1,
        };

        var relatedTerm = new DAL.Entities.Streetcode.TextContent.RelatedTerm
        {
            Id = 1,
            Word = "test",
            TermId = 1,
        };

        var command = new CreateRelatedTermCommand(createRelatedTermDTO);

        this.relatedTermRepositoryMock
            .Setup(r => r
                .CreateAsync(It.IsAny<DAL.Entities.Streetcode.TextContent.RelatedTerm>()))
            .ReturnsAsync(relatedTerm);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<
                    Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                        IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync([]);

        this.SetupSaveChangesMock(0);

        // Act
        var result = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(
            Messages.Error_FailedToCreateEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm))));
    }
}