namespace Streetcode.XUnitTest.MediatR.Comments.GetByStreetcodeId
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.GetByStreetcodeId;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Xunit;

    public class GetCommentsByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly GetCommentsByStreetcodeIdHandler handler;

        public GetCommentsByStreetcodeIdHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.commentRepositoryMock = new Mock<ICommentRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock.Setup(r => r.CommentRepository)
                .Returns(this.commentRepositoryMock.Object);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CommentProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new GetCommentsByStreetcodeIdHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOnlyRootComments_WhenHierarchyExists()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetCommentsByStreetcodeIdQuery(streetcodeId);

            var reply = new Comment { Id = 2, ParentCommentId = 1, TextContent = "Reply", StreetcodeId = streetcodeId };
            var rootWithReply = new Comment
            {
                Id = 1,
                ParentCommentId = null,
                TextContent = "Root with reply",
                StreetcodeId = streetcodeId,
                Replies = new List<Comment> { reply }
            };
            var standaloneRoot = new Comment { Id = 3, ParentCommentId = null, TextContent = "Standalone", StreetcodeId = streetcodeId };

            var comments = new List<Comment> { rootWithReply, reply, standaloneRoot };

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(comments);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().HaveCount(2);
            result.Value.Should().NotContain(c => c.Id == 2);

            // Verify nesting
            var rootResult = result.Value.First(c => c.Id == 1);
            rootResult.Replies.Should().HaveCount(1);
            rootResult.Replies.First().Id.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsSortedComments_WhenCommentsExist()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetCommentsByStreetcodeIdQuery(streetcodeId);

            var comments = new List<Comment>
            {
                new () { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-2), StreetcodeId = streetcodeId, TextContent = "Oldest" },
                new () { Id = 2, CreatedAt = DateTime.UtcNow, StreetcodeId = streetcodeId, TextContent = "Newest" },
                new () { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-1), StreetcodeId = streetcodeId, TextContent = "Middle" },
            };

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(comments);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var resultList = result.Value.ToList();
            resultList[0].TextContent.Should().Be("Newest");
            resultList[2].TextContent.Should().Be("Oldest");
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoCommentsFound()
        {
            // Arrange
            var query = new GetCommentsByStreetcodeIdQuery(1);

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<Comment>());

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}