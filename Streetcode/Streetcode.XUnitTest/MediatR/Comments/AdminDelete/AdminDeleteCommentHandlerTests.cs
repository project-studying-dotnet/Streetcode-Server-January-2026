namespace Streetcode.XUnitTest.MediatR.Comments.AdminDelete
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using global::MediatR;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Comments.AdminDelete;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class AdminDeleteCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly AdminDeleteCommentHandler handler;

        public AdminDeleteCommentHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.commentRepositoryMock = new Mock<ICommentRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock.Setup(r => r.CommentRepository)
                .Returns(this.commentRepositoryMock.Object);

            this.handler = new AdminDeleteCommentHandler(
                this.repositoryWrapperMock.Object,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCommentExists()
        {
            // Arrange
            int commentId = 1;
            var command = new AdminDeleteCommentCommand(commentId);

            var comment = new Comment { Id = commentId, StreetcodeId = 10 };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(comment);

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<Comment> { comment });

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            this.commentRepositoryMock.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<Comment>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenCommentNotFound()
        {
            // Arrange
            var command = new AdminDeleteCommentCommand(1);

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((Comment?)null);

            var expectedError = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), command.Id);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);

            this.commentRepositoryMock.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<Comment>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var command = new AdminDeleteCommentCommand(1);
            var comment = new Comment { Id = 1, StreetcodeId = 10 };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(comment);

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<Comment> { comment });

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            var expectedError = Messages.Error_FailedToDeleteEntity.Format(nameof(Comment));

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);

            this.commentRepositoryMock.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<Comment>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeletesAllDescendants_WhenCommentHasReplies()
        {
            // Arrange
            int targetCommentId = 1;
            int streetcodeId = 10;
            var command = new AdminDeleteCommentCommand(targetCommentId);

            var targetComment = new Comment { Id = targetCommentId, StreetcodeId = streetcodeId };
            var child1 = new Comment { Id = 2, StreetcodeId = streetcodeId, ParentId = targetCommentId };
            var child2 = new Comment { Id = 3, StreetcodeId = streetcodeId, ParentId = targetCommentId };
            var grandChild = new Comment { Id = 4, StreetcodeId = streetcodeId, ParentId = 2 };

            var allComments = new List<Comment> { targetComment, child1, child2, grandChild };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(targetComment);

            this.commentRepositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(allComments);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            this.commentRepositoryMock.Verify(
                x => x.DeleteRange(It.Is<IEnumerable<Comment>>(
                    commentsToDelete =>
                        commentsToDelete.Count() == 4 &&
                        commentsToDelete.Contains(targetComment) &&
                        commentsToDelete.Contains(child1) &&
                        commentsToDelete.Contains(child2) &&
                        commentsToDelete.Contains(grandChild))), Times.Once);
        }
    }
}