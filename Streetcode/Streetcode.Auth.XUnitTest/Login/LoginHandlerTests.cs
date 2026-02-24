namespace Streetcode.Auth.XUnitTest.Login
{
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.Interfaces;
    using Streetcode.Auth.BLL.Mapping;
    using Streetcode.Auth.BLL.MediatR.Login;
    using Streetcode.Auth.DAL.Entities;

    public class LoginHandlerTests
    {
        private const string Email = "test@example.com";
        private const string Password = "Password67!";
        private const string AccessToken = "test_access_token";
        private const string RefreshTokenStr = "test_refresh_token";

        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IConfiguration> configMock;
        private readonly IMapper mapper;
        private readonly LoginHandler handler;

        public LoginHandlerTests()
        {
            this.userManagerMock = this.CreateUserManagerMock();

            this.tokenServiceMock = new Mock<ITokenService>();
            this.configMock = new Mock<IConfiguration>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AuthProfile());
            });
            this.mapper = configuration.CreateMapper();

            this.handler = new LoginHandler(
                this.userManagerMock.Object,
                this.tokenServiceMock.Object,
                this.mapper,
                this.configMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", Email = Email, UserName = "TestUser" };
            var refreshTokenEntity = new RefreshToken { Token = RefreshTokenStr, UserId = "1" };

            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync(user);

            this.userManagerMock
                .Setup(m => m.CheckPasswordAsync(user, Password))
                .ReturnsAsync(true);

            this.tokenServiceMock
                .Setup(s => s.GenerateTokensAsync(user))
                .ReturnsAsync((AccessToken, refreshTokenEntity));

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

            var command = new LoginCommand(
                new LoginRequestDTO { Email = Email, Password = Password });

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var (responseDto, refreshToken) = result.Value;

            refreshToken.Should().Be(RefreshTokenStr);
            responseDto.AccessToken.Should().Be(AccessToken);
            responseDto.User.Email.Should().Be(Email);
            responseDto.AccessTokenExpiresAt
                .Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync((ApplicationUser?)null);

            var command = new LoginCommand(
                new LoginRequestDTO { Email = Email, Password = Password });

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Invalid email or password");

            this.tokenServiceMock.Verify(
                x => x.GenerateTokensAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenPasswordIsIncorrect()
        {
            // Arrange
            var user = new ApplicationUser { Email = Email };

            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync(user);

            this.userManagerMock
                .Setup(m => m.CheckPasswordAsync(user, Password))
                .ReturnsAsync(false);

            var command = new LoginCommand(
                new LoginRequestDTO { Email = Email, Password = Password });

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Invalid email or password");
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
