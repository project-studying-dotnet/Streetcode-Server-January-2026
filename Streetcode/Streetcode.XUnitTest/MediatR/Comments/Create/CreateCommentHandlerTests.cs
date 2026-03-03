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

            this.SetupStandardSuccessMocks(userId, createDto.TextContent);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TextContent.Should().Be(createDto.TextContent);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenReplyIsValid()
        {
            // Arrange
            int parentId = 1;
            var createDto = GetCreateCommentDTO();
            createDto.ParentCommentId = parentId;
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            this.commentRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Comment, bool>>>(), null, It.IsAny<bool>()))
                .ReturnsAsync(new Comment { Id = parentId });

            this.SetupStandardSuccessMocks(userId, createDto.TextContent);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ParentCommentId.Should().Be(parentId);
            this.commentRepositoryMock.Verify(x => x.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Comment, bool>>>(), null, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenParentCommentNotFound()
        {
            // Arrange
            int parentId = 999;
            var createDto = GetCreateCommentDTO();
            createDto.ParentCommentId = parentId;
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            this.commentRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Comment, bool>>>(), null, It.IsAny<bool>()))
                .ReturnsAsync((Comment)null);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Parent comment not found.");
            this.loggerMock.Verify(x => x.LogError(command, "Parent comment not found."), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenSaveChangesFails()
        {
            // Arrange
            var createDto = GetCreateCommentDTO();
            var userId = "user-123";
            var command = new CreateCommentCommand(createDto, userId);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            this.loggerMock.Verify(x => x.LogError(command, It.IsAny<string>()), Times.Once);
        }

        private void SetupStandardSuccessMocks(string userId, string textContent)
        {
            var commentEntity = new Comment
            {
                Id = 1,
                TextContent = textContent,
                UserId = userId,
            };

            this.commentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Comment>()))
                .ReturnsAsync(commentEntity);

            this.repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            this.userRepositoryMock.Setup(x => x.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(), null, It.IsAny<bool>()))
                .ReturnsAsync(new User { Id = userId, Name = "Name", Surname = "Surname" });
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