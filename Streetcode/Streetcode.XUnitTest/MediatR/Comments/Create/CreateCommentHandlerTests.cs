namespace Streetcode.XUnitTest.MediatR.Comments.Create
{
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Create;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class CreateCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly CreateCommentHandler handler;

        public CreateCommentHandlerTests()
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

            this.handler = new CreateCommentHandler(
                this.repositoryWrapperMock.Object,
                this.mapper,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRequestIsValid()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            var commentEntity = new Comment
            {
                Id = 1,
                TextContent = createDto.TextContent,
                StreetcodeId = createDto.StreetcodeId,
                UserId = userId,
            };

            this.commentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Comment>()))
                .ReturnsAsync(commentEntity);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TextContent.Should().Be(createDto.TextContent);
            result.Value.UserId.Should().Be(userId);

            this.commentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Comment>()), Times.Once);
            this.repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_SetsCorrectUserId_WhenRequestIsValid()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            var userId = "test-user-id";
            var command = new CreateCommentCommand(createDto, userId);

            Comment? capturedEntity = null;

            this.commentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Comment>()))
                .Callback<Comment>(c => capturedEntity = c);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity.Should().NotBeNull();
            capturedEntity!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            this.commentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Comment>()));

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0);

            var expectedErrorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(Comment));

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);

            this.loggerMock.Verify(x => x.LogError(command, expectedErrorMsg), Times.Once);
        }

        private static CreateCommentDTO GetCreateCommentDTO()
        {
            return new CreateCommentDTO
            {
                StreetcodeId = 1,
                TextContent = "This is a test comment",
            };
        }
    }
}
