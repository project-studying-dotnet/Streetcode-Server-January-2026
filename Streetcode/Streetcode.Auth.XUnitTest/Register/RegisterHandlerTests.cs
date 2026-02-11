namespace Streetcode.Auth.XUnitTest.Register
{
    using AutoMapper;
    using FluentAssertions;
    using MassTransit;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.Interfaces;
    using Streetcode.Auth.BLL.Mapping;
    using Streetcode.Auth.BLL.MediatR.Register;
    using Streetcode.Auth.DAL.Entities;
    using Streetcode.Auth.DAL.Enums;
    using Streetcode.Shared.DTO.Events;

    public class RegisterHandlerTests
    {
        private const string Email = "newuser@example.com";
        private const string Password = "Password123!";
        private const string NewId = "guid_new_user";
        private const string AccessToken = "test_access_token";
        private const string RefreshTokenStr = "test_refresh_token";

        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IConfiguration> configMock;
        private readonly Mock<IPublishEndpoint> publishEndpointMock;
        private readonly IMapper mapper;
        private readonly RegisterHandler handler;

        public RegisterHandlerTests()
        {
            this.userManagerMock = this.CreateUserManagerMock();
            this.tokenServiceMock = new Mock<ITokenService>();
            this.configMock = new Mock<IConfiguration>();
            this.publishEndpointMock = new Mock<IPublishEndpoint>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AuthProfile());
            });
            this.mapper = configuration.CreateMapper();

            this.handler = new RegisterHandler(
                this.userManagerMock.Object,
                this.mapper,
                this.tokenServiceMock.Object,
                this.configMock.Object,
                this.publishEndpointMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserAlreadyExists()
        {
            // Arrange
            var existingUser = new ApplicationUser { Email = Email };

            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync(existingUser);

            var command = this.CreateCommand();

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain($"User with email {Email} already exists");

            this.userManagerMock.Verify(
                x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenIdentityCreationFails()
        {
            // Arrange
            this.userManagerMock.Setup(m => m.FindByEmailAsync(Email)).ReturnsAsync((ApplicationUser?)null);

            var identityErrors = new IdentityError[] { new () { Description = "Password too weak" } };

            this.userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var command = this.CreateCommand();

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("Password too weak");
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessAndTokens_WhenRegistrationValid()
        {
            // Arrange
            this.SetupSuccessScenario();
            var command = this.CreateCommand();

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Item1.AccessToken.Should().Be(AccessToken);
            result.Value.Item2.Should().Be(RefreshTokenStr);

            result.Value.Item1.AccessTokenExpiresAt
                .Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task Handle_ShouldPublishRabbitMqEvent_WhenSuccess()
        {
            // Arrange
            this.SetupSuccessScenario();
            var command = this.CreateCommand();

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            this.publishEndpointMock.Verify(
                x => x.Publish(
                    It.Is<UserRegisteredEvent>(e =>
                        e.Email == Email &&
                        e.Name == "Test" &&
                        e.Surname == "User"),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldAssignUserRole_WhenSuccess()
        {
            // Arrange
            this.SetupSuccessScenario();
            var command = this.CreateCommand();

            // Act
            await this.handler.Handle(command, CancellationToken.None);

            // Assert
            this.userManagerMock.Verify(
                x => x.AddToRoleAsync(
                    It.Is<ApplicationUser>(u => u.Email == Email),
                    nameof(UserRole.User)), Times.Once);
        }

        private RegisterCommand CreateCommand()
        {
            return new RegisterCommand(new RegisterRequestDTO
            {
                Email = Email,
                Password = Password,
                Name = "Test",
                Surname = "User",
                PhoneNumber = "+380000000000",
            });
        }

        private void SetupSuccessScenario()
        {
            this.userManagerMock.Setup(
                m => m.FindByEmailAsync(Email)).ReturnsAsync((ApplicationUser?)null);

            this.userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), Password))
                .Callback<ApplicationUser, string>((u, p) => u.Id = NewId)
                .ReturnsAsync(IdentityResult.Success);

            this.userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var refreshTokenEntity = new RefreshToken { Token = RefreshTokenStr };
            this.tokenServiceMock
                .Setup(s => s.GenerateTokensAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((AccessToken, refreshTokenEntity));

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
