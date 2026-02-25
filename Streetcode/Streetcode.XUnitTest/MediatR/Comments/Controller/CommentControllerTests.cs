namespace Streetcode.XUnitTest.MediatR.Comments.Controller
{
    using System.Security.Claims;
    using FluentAssertions;
    using FluentResults;
    using global::MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Create;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Delete;
    using Streetcode.BLL.MediatR.Streetcode.Comments.GetByStreetcodeId;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Update;
    using Streetcode.WebApi.Controllers.Streetcode.Comments;
    using Xunit;

    public class CommentControllerTests
    {
        private readonly Mock<IMediator> mediatorMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly CommentController controller;

        public CommentControllerTests()
        {
            this.mediatorMock = new Mock<IMediator>();
            this.serviceProviderMock = new Mock<IServiceProvider>();

            this.serviceProviderMock
                .Setup(x => x.GetService(typeof(IMediator)))
                .Returns(this.mediatorMock.Object);

            this.controller = new CommentController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = this.serviceProviderMock.Object,
                    },
                },
            };
        }

        private void SetupUser(string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[]
                {
                    new (ClaimTypes.NameIdentifier, userId),
                }, "mock"));

            this.controller.ControllerContext.HttpContext.User = user;
        }

        [Fact]
        public async Task GetByStreetcodeId_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            int streetcodeId = 1;
            var commentsList = new List<CommentDTO>
            {
                new () { Id = 1, TextContent = "Test" },
            };

            this.mediatorMock.Setup(m => m.Send(It.IsAny<GetCommentsByStreetcodeIdQuery>(), default))
                .ReturnsAsync(Result.Ok((IEnumerable<CommentDTO>)commentsList));

            // Act
            var result = await this.controller.GetByStreetcodeId(streetcodeId);

            // Assert
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            // Arrange
            var createDto = new CreateCommentDTO { TextContent = "Test" };

            // Act
            var result = await this.controller.Create(createDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var userId = "user-guid-123";
            this.SetupUser(userId);

            var createDto = new CreateCommentDTO { TextContent = "Test" };
            var returnedDto = new CommentDTO { Id = 1, TextContent = "Test" };

            this.mediatorMock.Setup(m => m.Send(It.IsAny<CreateCommentCommand>(), default))
                .ReturnsAsync(Result.Ok(returnedDto));

            // Act
            var result = await this.controller.Create(createDto);

            // Assert
            this.mediatorMock.Verify(
                m => m.Send(It.Is<CreateCommentCommand>(c => c.UserId == userId), default), Times.Once);
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            // Arrange
            var updateDto = new UpdateCommentDTO { Id = 1 };

            // Act
            var result = await this.controller.Update(updateDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var userId = "user-guid-123";
            this.SetupUser(userId);

            var updateDto = new UpdateCommentDTO { Id = 1, TextContent = "New Text" };
            var returnedDto = new CommentDTO { Id = 1, TextContent = "New Text" };

            this.mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCommentCommand>(), default))
                .ReturnsAsync(Result.Ok(returnedDto));

            // Act
            var result = await this.controller.Update(updateDto);

            // Assert
            this.mediatorMock.Verify(
                m => m.Send(It.Is<UpdateCommentCommand>(c => c.UserId == userId), default), Times.Once);
            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            // Arrange
            int id = 1;

            // Act
            var result = await this.controller.Delete(id);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var userId = "user-guid-123";
            this.SetupUser(userId);
            int id = 1;

            this.mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCommentCommand>(), default))
                .ReturnsAsync(Result.Ok(Unit.Value));

            // Act
            var result = await this.controller.Delete(id);

            // Assert
            this.mediatorMock.Verify(
                m => m.Send(It.Is<DeleteCommentCommand>(c => c.UserId == userId), default), Times.Once);
            result.Should().BeAssignableTo<IActionResult>();
        }
    }
}
