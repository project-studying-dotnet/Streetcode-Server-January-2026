namespace Streetcode.Auth.XUnitTest.LoginWithGoogle
{
    using AutoMapper;
    using FluentAssertions;
    using MassTransit;
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
    using Streetcode.Shared.DTO.Events;
    using Xunit;

    public class LoginWithGoogleHandlerTests
    {
        private const string Email = "google-user@gmail.com";
        private const string Name = "John";
        private const string Surname = "Smith";
        private const string AccessToken = "google_access_token";
        private const string RefreshTokenStr = "google_refresh_token";

        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IConfiguration> configMock;
        private readonly Mock<ILogger<LoginWithGoogleHandler>> loggerMock;
        private readonly Mock<IPublishEndpoint> publishEndpointMock;
        private readonly IMapper mapper;
        private readonly LoginWithGoogleHandler handler;

        public LoginWithGoogleHandlerTests()
        {
            this.userManagerMock = this.CreateUserManagerMock();
            this.tokenServiceMock = new Mock<ITokenService>();
            this.configMock = new Mock<IConfiguration>();
            this.loggerMock = new Mock<ILogger<LoginWithGoogleHandler>>();
            this.publishEndpointMock = new Mock<IPublishEndpoint>();

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
                this.configMock.Object,
                this.publishEndpointMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "google-id-123",
                Email = Email,
                UserName = Email,
                Name = Name,
                Surname = Surname
            };
            var refreshTokenEntity = new RefreshToken { Token = RefreshTokenStr, UserId = user.Id };

            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync(user);

            this.tokenServiceMock
                .Setup(s => s.GenerateTokensAsync(user))
                .ReturnsAsync((AccessToken, refreshTokenEntity));

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

            var command = new LoginWithGoogleCommand(new LoginWithGoogleDTO
            {
                Email = Email,
                Name = Name,
                Surname = Surname
            });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var (responseDto, refreshToken) = result.Value;

            refreshToken.Should().Be(RefreshTokenStr);
            responseDto.AccessToken.Should().Be(AccessToken);
            responseDto.User.Email.Should().Be(Email);
            responseDto.User.Surname.Should().Be(Surname);

            this.tokenServiceMock.Verify(s => s.GenerateTokensAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateUserAndReturnSuccess_WhenUserDoesNotExist()
        {
            // Arrange
            this.userManagerMock
                .Setup(m => m.FindByEmailAsync(Email))
                .ReturnsAsync((ApplicationUser?)null);

            this.userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            this.userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            var user = new ApplicationUser { Id = "new-id", Email = Email };
            var refreshTokenEntity = new RefreshToken { Token = RefreshTokenStr, UserId = user.Id };

            this.tokenServiceMock
                .Setup(s => s.GenerateTokensAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((AccessToken, refreshTokenEntity));

            this.configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

            var command = new LoginWithGoogleCommand(new LoginWithGoogleDTO
            {
                Email = Email,
                Name = "New",
                Surname = "User"
            });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            this.userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
            this.publishEndpointMock.Verify(p => p.Publish(It.IsAny<UserRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}