namespace Streetcode.Auth.XUnitTest.LoginWithGoogle
{
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.DTO.Users;
    using Streetcode.Auth.BLL.Interfaces;
    using Streetcode.Auth.BLL.Mapping;
    using Streetcode.Auth.BLL.MediatR.LoginWithGoogle;
    using Streetcode.Auth.DAL.Entities;
    using Xunit;

    public class LoginWithGoogleHandlerTests
    {
        private const string Email = "google-user@gmail.com";
        private const string AccessToken = "google_access_token";
        private const string RefreshTokenStr = "google_refresh_token";

        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IConfiguration> configMock;
        private readonly Mock<ILogger<LoginWithGoogleHandler>> loggerMock;
        private readonly IMapper mapper;
        private readonly LoginWithGoogleHandler handler;

        public LoginWithGoogleHandlerTests()
        {
            this.userManagerMock = this.CreateUserManagerMock();
            this.tokenServiceMock = new Mock<ITokenService>();
            this.configMock = new Mock<IConfiguration>();
            this.loggerMock = new Mock<ILogger<LoginWithGoogleHandler>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AuthProfile());
            });
            this.mapper = configuration.CreateMapper();

            this.handler = new LoginWithGoogleHandler(
                this.loggerMock.Object,
                this.tokenServiceMock.Object,
                this.userManagerMock.Object,
                this.mapper,
                this.configMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = "google-id-123", Email = Email, UserName = Email };
            var refreshTokenEntity = new RefreshToken { Token = RefreshTokenStr, UserId = user.Id };

            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync(user);

            this.tokenServiceMock
                .Setup(s => s.GenerateTokensAsync(user))
                .ReturnsAsync((AccessToken, refreshTokenEntity));

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

            var command = new LoginWithGoogleCommand(new LoginWithGoogleDTO { Email = Email, Name = "Google User" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var (responseDto, refreshToken) = result.Value;

            refreshToken.Should().Be(RefreshTokenStr);
            responseDto.AccessToken.Should().Be(AccessToken);
            responseDto.User.Email.Should().Be(Email);

            this.tokenServiceMock.Verify(s => s.GenerateTokensAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserDoesNotExist()
        {
            // Arrange
            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync((ApplicationUser?)null);

            var command = new LoginWithGoogleCommand(new LoginWithGoogleDTO { Email = Email, Name = "New User" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "UserNotFoundRegistrationRequired");

            this.tokenServiceMock.Verify(s => s.GenerateTokensAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}