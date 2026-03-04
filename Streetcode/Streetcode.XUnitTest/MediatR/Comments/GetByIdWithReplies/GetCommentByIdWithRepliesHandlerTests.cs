namespace Streetcode.XUnitTest.MediatR.Comments.GetByIdWithReplies
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.GetByIdWithReplies;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class GetCommentByIdWithRepliesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly GetCommentByIdWithRepliesHandler handler;

        public GetCommentByIdWithRepliesHandlerTests()
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

            this.handler = new GetCommentByIdWithRepliesHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCommentExistsAndHasNoReplies()
        {
            // Arrange
            int targetId = 1;
            int streetcodeId = 10;
            var query = new GetCommentByIdWithRepliesQuery(targetId);

            var targetComment = new Comment { Id = targetId, StreetcodeId = streetcodeId, CreatedAt = DateTime.UtcNow };
            var allComments = new List<Comment> { targetComment };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(targetComment);

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(allComments);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value.Id.Should().Be(targetId);
            result.Value.Replies.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_BuildsTreeAndSortsReplies_WhenCommentHasReplies()
        {
            // Arrange
            int targetId = 1;
            int streetcodeId = 10;
            var query = new GetCommentByIdWithRepliesQuery(targetId);
            var baseTime = DateTime.UtcNow;

            var targetComment = new Comment
            {
                Id = targetId,
                StreetcodeId = streetcodeId,
                CreatedAt = baseTime,
            };
            var child1 = new Comment
            {
                Id = 2,
                StreetcodeId = streetcodeId,
                ParentId = targetId,
                CreatedAt = baseTime.AddMinutes(10),
            };
            var child2 = new Comment
            {
                Id = 3,
                StreetcodeId = streetcodeId,
                ParentId = targetId,
                CreatedAt = baseTime.AddMinutes(5),
            };
            var grandChild = new Comment
            {
                Id = 4,
                StreetcodeId = streetcodeId,
                ParentId = 3,
                CreatedAt = baseTime.AddMinutes(15),
            };
            var otherComment = new Comment
            {
                Id = 5,
                StreetcodeId = streetcodeId,
            };

            var allComments = new List<Comment> { targetComment, child1, child2, grandChild, otherComment };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(targetComment);

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(allComments);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var resultComment = result.Value;
            resultComment.Id.Should().Be(targetId);
            resultComment.Replies.Should().HaveCount(2);

            resultComment.Replies[0].Id.Should().Be(3);
            resultComment.Replies[1].Id.Should().Be(2);

            resultComment.Replies[0].Replies.Should().HaveCount(1);
            resultComment.Replies[0].Replies[0].Id.Should().Be(4);

            resultComment.Replies[1].Replies.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenCommentNotFound()
        {
            // Arrange
            var query = new GetCommentByIdWithRepliesQuery(99);

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((Comment?)null);

            var expectedError = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), query.Id);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);
        }

    }
}