namespace Streetcode.XUnitTest.MediatR.Comments.GetByStreetcodeId
{
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.GetByStreetcodeId;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Enums;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using System.Linq.Expressions;
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
        public async Task Handle_ReturnsSortedComments_WhenCommentsExist()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetCommentsByStreetcodeIdQuery(streetcodeId);

            var comments = new List<Comment>
            {
                new ()
                {
                    Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-2),
                    StreetcodeId = streetcodeId, TextContent = "Oldest",
                },
                new ()
                {
                    Id = 2, CreatedAt = DateTime.UtcNow,
                    StreetcodeId = streetcodeId, TextContent = "Newest",
                },
                new ()
                {
                    Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-1),
                    StreetcodeId = streetcodeId, TextContent = "Middle",
                },
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
            result.Value.Should().HaveCount(3);

            var resultList = result.Value.ToList();
            resultList[0].TextContent.Should().Be("Newest");
            resultList[1].TextContent.Should().Be("Middle");
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
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_BuildsTreeAndSortsRepliesCorrectly_WhenCommentsHaveHierarchy()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetCommentsByStreetcodeIdQuery(streetcodeId);
            var baseTime = DateTime.UtcNow;

            var comments = new List<Comment>
            {
                new ()
                {
                    Id = 1,
                    StreetcodeId = streetcodeId,
                    ParentId = null,
                    CreatedAt = baseTime,
                    Status = CommentStatus.Approved,
                },
                new ()
                {
                    Id = 2,
                    StreetcodeId = streetcodeId,
                    ParentId = 1,
                    CreatedAt = baseTime.AddMinutes(10),
                    Status = CommentStatus.Approved,
                },
                new ()
                {
                    Id = 3,
                    StreetcodeId = streetcodeId,
                    ParentId = 1,
                    CreatedAt = baseTime.AddMinutes(5),
                    Status = CommentStatus.Approved,
                },
                new ()
                {
                    Id = 4,
                    StreetcodeId = streetcodeId,
                    ParentId = 3,
                    CreatedAt = baseTime.AddMinutes(15),
                    Status = CommentStatus.Approved,
                },
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
            var rootComments = result.Value.ToList();

            rootComments.Should().HaveCount(1);
            var root = rootComments[0];
            root.Id.Should().Be(1);

            root.Replies.Should().HaveCount(2);
            root.Replies[0].Id.Should().Be(3);
            root.Replies[1].Id.Should().Be(2);

            root.Replies[0].Replies.Should().HaveCount(1);
            root.Replies[0].Replies[0].Id.Should().Be(4);

            root.Replies[1].Replies.Should().BeEmpty();
        }
    }
}
