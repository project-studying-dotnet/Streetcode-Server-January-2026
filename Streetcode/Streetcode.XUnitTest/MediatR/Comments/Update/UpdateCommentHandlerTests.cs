namespace Streetcode.XUnitTest.MediatR.Comments.Update
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Update;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class UpdateCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly UpdateCommentHandler handler;

        public UpdateCommentHandlerTests()
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

            this.handler = new UpdateCommentHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenUserIsOwnerAndCommentExists()
        {
            // Arrange
            var updateDto = new UpdateCommentDTO { Id = 1, TextContent = "Updated Text" };
            var userId = "user-123";
            var command = new UpdateCommentCommand(updateDto, userId);

            var existingComment = new Comment
            {
                Id = 1,
                UserId = userId,
                TextContent = "Old Text",
                StreetcodeId = 1,
            };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(existingComment);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TextContent.Should().Be("Updated Text");

            this.commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenCommentNotFound()
        {
            // Arrange
            var updateDto = new UpdateCommentDTO { Id = 999, TextContent = "Text" };
            var command = new UpdateCommentCommand(updateDto, "user-123");

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((Comment?)null);

            var expectedError = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), updateDto.Id);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);

            this.commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenUserIsNotOwner()
        {
            // Arrange
            var updateDto = new UpdateCommentDTO { Id = 1, TextContent = "Hacked Text" };
            var ownerId = "owner";
            var hackerId = "hacker";

            var command = new UpdateCommentCommand(updateDto, hackerId);

            var existingComment = new Comment { Id = 1, UserId = ownerId };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(existingComment);

            var expectedError = Messages.Error_UserNotCommentOwner;

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);

            this.commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdatesDateAndText_WhenUpdating()
        {
            // Arrange
            var updateDto = new UpdateCommentDTO { Id = 1, TextContent = "New Content" };
            var userId = "user-123";
            var command = new UpdateCommentCommand(updateDto, userId);

            var existingComment = new Comment { Id = 1, UserId = userId, TextContent = "Old Content" };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(existingComment);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            Comment? capturedEntity = null;

            this.commentRepositoryMock.Setup(x => x.Update(It.IsAny<Comment>()))
                .Callback<Comment>(c => capturedEntity = c);

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.TextContent.Should().Be("New Content");
            capturedEntity.UpdatedAt.Should().NotBeNull();
            capturedEntity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var updateDto = new UpdateCommentDTO { Id = 1, TextContent = "Text" };
            var userId = "user-123";
            var command = new UpdateCommentCommand(updateDto, userId);

            var existingComment = new Comment { Id = 1, UserId = userId };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(existingComment);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            var expectedError = Messages.Error_FailedToUpdateEntity.Format(nameof(Comment));

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);
        }
    }
}
