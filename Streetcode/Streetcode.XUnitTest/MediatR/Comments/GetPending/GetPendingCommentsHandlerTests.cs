namespace Streetcode.XUnitTest.MediatR.Comments.GetPending
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.GetPending;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Enums;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Xunit;

    public class GetPendingCommentsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly GetPendingCommentsHandler handler;

        public GetPendingCommentsHandlerTests()
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

            this.handler = new GetPendingCommentsHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSortedComments_WhenPendingCommentsExist()
        {
            // Arrange
            var query = new GetPendingCommentsQuery();
            var baseTime = DateTime.UtcNow;

            var comments = new List<Comment>
            {
                new ()
                {
                    Id = 1,
                    CreatedAt = baseTime.AddDays(-1),
                    Status = CommentStatus.Pending,
                    TextContent = "Middle",
                },
                new ()
                {
                    Id = 2,
                    CreatedAt = baseTime,
                    Status = CommentStatus.Pending,
                    TextContent = "Newest",
                },
                new ()
                {
                    Id = 3,
                    CreatedAt = baseTime.AddDays(-2),
                    Status = CommentStatus.Pending,
                    TextContent = "Oldest",
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
            resultList[0].TextContent.Should().Be("Oldest");
            resultList[1].TextContent.Should().Be("Middle");
            resultList[2].TextContent.Should().Be("Newest");
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoPendingCommentsFound()
        {
            // Arrange
            var query = new GetPendingCommentsQuery();

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
    }
}