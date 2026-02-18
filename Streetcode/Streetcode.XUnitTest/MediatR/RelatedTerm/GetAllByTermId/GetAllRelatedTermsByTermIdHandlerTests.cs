using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.GetAllByTermId;

public class GetAllRelatedTermsByTermIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly Mock<IRelatedTermRepository> relatedTermRepositoryMock;
    private readonly IMapper mapper;
    private readonly GetAllRelatedTermsByTermIdHandler handler;

    public GetAllRelatedTermsByTermIdHandlerTests()
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

        this.handler = new GetAllRelatedTermsByTermIdHandler(
            this.mapper,
            this.repositoryWrapperMock.Object,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsRelatedTerms_WhenRelatedTermsByTermIdExist()
    {
        const int termId = 1;
        var relatedTerms = new List<DAL.Entities.Streetcode.TextContent.RelatedTerm>
        {
            new ()
            {
                Id = 1,
                TermId = 1,
                Word = "test",
            },
            new ()
            {
                Id = 2,
                TermId = 1,
                Word = "test2",
            },
        };

        var relatedTermDtos = new List<RelatedTermDTO>
        {
            new ()
            {
                Id = 1,
                TermId = 1,
                Word = "test",
            },
            new ()
            {
                Id = 2,
                TermId = 1,
                Word = "test2",
            },
        };

        var query = new GetAllRelatedTermsByTermIdQuery(termId);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(relatedTerms);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(relatedTermDtos);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenRelatedTermsByIdNotExist()
    {
        // Arrange
        const int termId = 1;
        var query = new GetAllRelatedTermsByTermIdQuery(termId);

        this.relatedTermRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm>,
                    IIncludableQueryable<DAL.Entities.Streetcode.TextContent.RelatedTerm, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync([]);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(
            Messages.Error_RelatedTermsByTermIdNotFound.Format(termId)));
    }
}