namespace Streetcode.XUnitTest.MediatR.Comments.Create
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Mapping.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Create;
    using Streetcode.DAL.Entities.Streetcode.Comments;
    using Streetcode.DAL.Entities.Users;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.Comments;
    using Streetcode.DAL.Repositories.Interfaces.Users;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class CreateCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ICommentRepository> commentRepositoryMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly IMapper mapper;
        private readonly CreateCommentHandler handler;

        public CreateCommentHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.commentRepositoryMock = new Mock<ICommentRepository>();
            this.userRepositoryMock = new Mock<IUserRepository>();
            this.loggerMock = new Mock<ILoggerService>();

            this.repositoryWrapperMock.Setup(r => r.CommentRepository)
                .Returns(this.commentRepositoryMock.Object);

            this.repositoryWrapperMock.Setup(r => r.UserRepository)
                .Returns(this.userRepositoryMock.Object);

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

            this.userRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(new User());

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TextContent.Should().Be(createDto.TextContent);

            this.commentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Comment>()), Times.Once);
            this.repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_SetsCorrectUser_WhenRequestIsValid()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            var userId = "test-user-id";
            var command = new CreateCommentCommand(createDto, userId);

            var user = new User
            {
                Id = userId,
                Name = "TestName",
                Surname = "TestSurname",
            };

            this.commentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Comment>()))
                .ReturnsAsync((Comment c) => c);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            this.userRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.Value.UserId.Should().Be(userId);
            result.Value.UserFullName.Should().Be("TestName TestSurname");
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenParentCommentExists()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            createDto.ParentId = 5;
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            var parentComment = new Comment { Id = 5, StreetcodeId = createDto.StreetcodeId };

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(parentComment);

            this.commentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Comment>()))
                .ReturnsAsync((Comment c) => c);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            this.commentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenParentCommentDoesNotExist()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            createDto.ParentId = 999;
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            this.commentRepositoryMock
                .Setup(x => x.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Comment, bool>>>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync((Comment?)null);

            var expectedErrorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), createDto.ParentId);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(expectedErrorMsg);
            this.commentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Comment>()), Times.Never);
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
