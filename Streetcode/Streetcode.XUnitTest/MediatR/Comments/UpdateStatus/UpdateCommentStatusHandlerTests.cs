namespace Streetcode.XUnitTest.MediatR.Comments.UpdateStatus
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Enums;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class UpdateCommentStatusHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly UpdateCommentStatusHandler handler;

        public UpdateCommentStatusHandlerTests()
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

            this.handler = new UpdateCommentStatusHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCommentExistsAndSaved()
        {
            // Arrange
            var dto = new UpdateCommentStatusDTO { Id = 1, Status = CommentStatus.Approved };
            var command = new UpdateCommentStatusCommand(dto);

            var existingComment = new Comment { Id = 1, Status = CommentStatus.Pending };

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
            result.Value.Id.Should().Be(existingComment.Id);

            existingComment.Status.Should().Be(CommentStatus.Approved);

            this.commentRepositoryMock.Verify(x => x.Update(existingComment), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenCommentNotFound()
        {
            // Arrange
            var dto = new UpdateCommentStatusDTO { Id = 99, Status = CommentStatus.Rejected };
            var command = new UpdateCommentStatusCommand(dto);

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((Comment?)null);

            var expectedError = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), dto.Id);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedError);

            this.commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Never);
            this.repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var dto = new UpdateCommentStatusDTO { Id = 1, Status = CommentStatus.Approved };
            var command = new UpdateCommentStatusCommand(dto);

            var existingComment = new Comment { Id = 1, Status = CommentStatus.Pending };

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

            this.commentRepositoryMock.Verify(x => x.Update(existingComment), Times.Once);
        }
    }
}