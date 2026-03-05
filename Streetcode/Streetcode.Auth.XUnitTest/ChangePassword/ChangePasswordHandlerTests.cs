using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.BLL.MediatR.ChangePassword;
using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.XUnitTest.ChangePassword
{
    public class ChangePasswordHandlerTests
    {
        private const string Email = "user@example.com";
        private const string UserId = "user-id-123";
        private const string CurrentPassword = "OldPassword123!";
        private const string NewPassword = "NewPassword456!";

        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly ChangePasswordHandler handler;

        public ChangePasswordHandlerTests()
        {
            this.userManagerMock = this.CreateUserManagerMock();
            this.tokenServiceMock = new Mock<ITokenService>();

            this.handler = new ChangePasswordHandler(
                this.userManagerMock.Object,
                this.tokenServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessAndRevokeTokens_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new ApplicationUser { Id = UserId, Email = Email };
            this.userManagerMock.Setup(m => m.FindByEmailAsync(Email)).ReturnsAsync(user);

            this.userManagerMock.Setup(m => m.ChangePasswordAsync(user, CurrentPassword, NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var command = new ChangePasswordCommand(new ChangePasswordRequestDTO
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword
            }, Email);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            this.tokenServiceMock.Verify(s => s.RevokeAllAsync(UserId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            this.userManagerMock.Setup(m => m.FindByEmailAsync(Email)).ReturnsAsync((ApplicationUser?)null);

            var command = new ChangePasswordCommand(new ChangePasswordRequestDTO
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword
            }, Email);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("User not found");

            this.tokenServiceMock.Verify(s => s.RevokeAllAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenIdentityChangePasswordFails()
        {
            // Arrange
            var user = new ApplicationUser { Id = UserId, Email = Email };
            var identityErrors = new[] { new IdentityError { Description = "Incorrect current password" } };

            this.userManagerMock.Setup(m => m.FindByEmailAsync(Email)).ReturnsAsync(user);
            this.userManagerMock.Setup(m => m.ChangePasswordAsync(user, CurrentPassword, NewPassword))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var command = new ChangePasswordCommand(new ChangePasswordRequestDTO
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword
            }, Email);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Incorrect current password");

            this.tokenServiceMock.Verify(s => s.RevokeAllAsync(It.IsAny<string>()), Times.Never);
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}