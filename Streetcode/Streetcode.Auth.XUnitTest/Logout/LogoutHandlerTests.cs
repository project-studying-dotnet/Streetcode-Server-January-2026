namespace Streetcode.Auth.XUnitTest.Logout
{
    using FluentAssertions;
    using MediatR;
    using Moq;
    using Streetcode.Auth.BLL.Interfaces;
    using Streetcode.Auth.BLL.MediatR.Logout;

    public class LogoutHandlerTests
    {
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly LogoutHandler handler;

        public LogoutHandlerTests()
        {
            this.tokenServiceMock = new Mock<ITokenService>();

            this.handler = new LogoutHandler(this.tokenServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var refreshToken = "some-test-refresh-token-123";
            var command = new LogoutCommand(refreshToken);

            this.tokenServiceMock
                .Setup(s => s.RevokeRefreshTokenAsync(refreshToken))
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            this.tokenServiceMock.Verify(
                x => x.RevokeRefreshTokenAsync(refreshToken),
                Times.Once);
        }
    }
}
