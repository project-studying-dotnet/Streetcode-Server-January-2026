namespace Streetcode.XUnitTest.MediatR.Comments.Delete
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using global::MediatR;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Delete;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class DeleteCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly DeleteCommentHandler handler;

        public DeleteCommentHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.commentRepositoryMock = new Mock<ICommentRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock.Setup(r => r.CommentRepository)
                .Returns(this.commentRepositoryMock.Object);

            this.handler = new DeleteCommentHandler(
                this.repositoryWrapperMock.Object,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenUserIsOwnerAndCommentExists()
        {
            // Arrange
            int commentId = 1;
            string userId = "user-123";
            var command = new DeleteCommentCommand(commentId, userId);

            var comment = new Comment { Id = commentId, UserId = userId, Replies = new List<Comment>() };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(), 
                    It.IsAny<bool>()))
                .ReturnsAsync(comment);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            this.commentRepositoryMock.Verify(x => x.Delete(comment), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenCommentNotFound()
        {
            // Arrange
            var command = new DeleteCommentCommand(1, "user-123");

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Comment?)null);

            var expectedError = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), command.Id);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenUserIsNotOwner()
        {
            // Arrange
            int commentId = 1;
            string ownerId = "owner";
            string hackerId = "hacker";

            var command = new DeleteCommentCommand(commentId, hackerId);
            var comment = new Comment { Id = commentId, UserId = ownerId };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(comment);

            var expectedError = Messages.Error_UserNotCommentOwner;

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var command = new DeleteCommentCommand(1, "user-123");
            var comment = new Comment { Id = 1, UserId = "user-123" };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<Comment>, IIncludableQueryable<Comment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(comment);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
        }
    }
}