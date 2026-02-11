namespace Streetcode.Auth.XUnitTest.RefreshToken
{
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Streetcode.Auth.BLL.Interfaces;
    using Streetcode.Auth.BLL.Mapping;
    using Streetcode.Auth.BLL.MediatR.RefreshToken;
    using Streetcode.Auth.DAL.Entities;

    public class RefreshTokenHandlerTests
    {
        private const string OldToken = "old_token";
        private const string NewAccess = "new_access_token";
        private const string NewRefresh = "new_refresh_token";
        private const string UserId = "user_id_1";
        private const string UserEmail = "test@example.com";

        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IConfiguration> configMock;
        private readonly IMapper mapper;
        private readonly RefreshTokenHandler handler;

        public RefreshTokenHandlerTests()
        {
            this.tokenServiceMock = new Mock<ITokenService>();
            this.configMock = new Mock<IConfiguration>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AuthProfile());
            });

            this.mapper = configuration.CreateMapper();

            this.handler = new RefreshTokenHandler(
                this.tokenServiceMock.Object,
                this.mapper,
                this.configMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessAndCorrectTokens_WhenRequestIsValid()
        {
            // Arrange
            this.SetupSuccessScenario();
            var command = new RefreshTokenCommand(OldToken);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Item1.AccessToken.Should().Be(NewAccess);
            result.Value.Item2.Should().Be(NewRefresh);
        }

        [Fact]
        public async Task Handle_ShouldMapUserCorrectly_WhenRequestIsValid()
        {
            // Arrange
            this.SetupSuccessScenario();
            var command = new RefreshTokenCommand(OldToken);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            var userDto = result.Value.Item1.User;

            userDto.Should().NotBeNull();
            userDto.Id.Should().Be(UserId);
            userDto.Email.Should().Be(UserEmail);
        }

        [Fact]
        public async Task Handle_ShouldSetCorrectExpirationTime_WhenRequestIsValid()
        {
            // Arrange
            this.SetupSuccessScenario();

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("120");

            var command = new RefreshTokenCommand(OldToken);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            var expiresAt = result.Value.Item1.AccessTokenExpiresAt;
            expiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(120), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenServiceThrowsException()
        {
            // Arrange
            var errorMsg = "Token invalid";

            this.tokenServiceMock
                .Setup(s => s.RotateRefreshTokenAsync(OldToken))
                .ThrowsAsync(new Exception(errorMsg));

            var command = new RefreshTokenCommand(OldToken);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(errorMsg);
        }

        private void SetupSuccessScenario()
        {
            var user = new ApplicationUser { Id = UserId, Email = UserEmail };
            var refreshTokenEntity = new RefreshToken { Token = NewRefresh, User = user };

            this.tokenServiceMock
                .Setup(s => s.RotateRefreshTokenAsync(OldToken))
                .ReturnsAsync((NewAccess, refreshTokenEntity));

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        }
    }
}
