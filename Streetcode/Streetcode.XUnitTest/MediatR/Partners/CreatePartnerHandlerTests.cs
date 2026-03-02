namespace Streetcode.XUnitTest.MediatR.Partners;

using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Streetcode.BLL.DTO.Partners.Create;
using Streetcode.BLL.Mapping.Streetcode;
using FluentAssertions;
using Streetcode.BLL.Mapping.Partners;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

public class CreatePartnerHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<IPartnerSourceLinkRepository> partnerSourceLinkRepositoryMock;
    private readonly Mock<IStreetcodeRepository> streetcodeRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly CreatePartnerHandler createPartnerHandler;

    public CreatePartnerHandlerTests()
    {
        this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        this.partnersRepositoryMock = new Mock<IPartnersRepository>();
        this.partnerSourceLinkRepositoryMock = new Mock<IPartnerSourceLinkRepository>();
        this.streetcodeRepositoryMock = new Mock<IStreetcodeRepository>();
        this.loggerMock = new Mock<ILoggerService>();

        this.repositoryWrapperMock
            .Setup(r => r.PartnersRepository)
            .Returns(this.partnersRepositoryMock.Object);

        this.repositoryWrapperMock
            .Setup(r => r.PartnerSourceLinkRepository)
            .Returns(this.partnerSourceLinkRepositoryMock.Object);

        this.repositoryWrapperMock
            .Setup(r => r.StreetcodeRepository)
            .Returns(this.streetcodeRepositoryMock.Object);

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new PartnerProfile());
            cfg.AddProfile(new PartnerSourceLinkProfile());
            cfg.AddProfile(new StreetcodeProfile());
        });
        this.mapper = new Mapper(configuration);

        this.createPartnerHandler = new CreatePartnerHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    private void SetupGetAllStreetcodesMock(List<StreetcodeContent> streetcodes)
    {
        this.streetcodeRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>,
                    IIncludableQueryable<StreetcodeContent, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(streetcodes);
    }

    private void SetupSaveChangesMock(int result)
    {
        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Handle_ReturnsCreatedPartner_WhenCreateSuccessful()
    {
        // Arrange
        var createPartnerDto = new CreatePartnerDTO
        {
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
            Streetcodes = [
                new StreetcodeShortDTO
                {
                    Id = 1,
                    Title = "Test Streetcode",
                },
            ],
            PartnerSourceLinks = [
                new CreatePartnerSourceLinkDTO
                {
                    LogoType = 0,
                    TargetUrl = "http://sourcelink1-url.com",
                },
                new CreatePartnerSourceLinkDTO
                {
                    LogoType = 0,
                    TargetUrl = "http://sourcelink2-url.com",
                },
            ],
        };

        var partner = new Partner
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
        };

        var streetcode = new StreetcodeContent()
        {
            Id = 1,
            Title = "Test Streetcode",
        };

        var expectedPartnerDto = new PartnerDTO()
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            LogoId = 1,
            TargetUrl = new UrlDTO
            {
                Title = "test-title",
                Href = "http://partner-url.com",
            },
            PartnerSourceLinks =
            [
                new PartnerSourceLinkDTO
                {
                    Id = 1,
                    LogoType = 0,
                    TargetUrl = new UrlDTO
                    {
                        Href = "http://sourcelink1-url.com",
                    },
                },
                new PartnerSourceLinkDTO
                {
                    Id = 2,
                    LogoType = 0,
                    TargetUrl = new UrlDTO
                    {
                        Href = "http://sourcelink2-url.com",
                    },
                },
            ],
            Streetcodes =
            [
                new StreetcodeShortDTO
                {
                    Id = 1,
                    Title = "Test Streetcode",
                },
            ],
        };

        var command = new CreatePartnerCommand(createPartnerDto);

        this.SetupGetAllStreetcodesMock([streetcode]);

        this.partnerSourceLinkRepositoryMock
            .Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<PartnerSourceLink>>()))
            .Callback<IEnumerable<PartnerSourceLink>>(links =>
            {
                links = links.ToList();
                links.First().Id = 1;
                links.Skip(1).First().Id = 2;
            })
            .Returns(Task.CompletedTask);

        this.repositoryWrapperMock
            .Setup(r => r.PartnersRepository.CreateAsync(It.IsAny<Partner>()))
            .Callback<Partner>(p => p.Id = 1)
            .ReturnsAsync(partner);

        this.SetupSaveChangesMock(1);

        // Act
        var result = await this.createPartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Should().BeEquivalentTo(expectedPartnerDto);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenLogoIdIsLessThanOne()
    {
        // Arrange
        var createPartnerDto = new CreatePartnerDTO
        {
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 0,
            UrlTitle = "test-title",
        };

        var command = new CreatePartnerCommand(createPartnerDto);
        var errorMsg = Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(createPartnerDto.LogoId));

        // Act
        var result = await this.createPartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenSaveUnsuccessful()
    {
        // Arrange
        var createPartnerDto = new CreatePartnerDTO
        {
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
        };

        var partner = new Partner
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
        };

        var command = new CreatePartnerCommand(createPartnerDto);
        var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(Partner));

        this.SetupGetAllStreetcodesMock([]);

        this.repositoryWrapperMock
            .Setup(r => r.PartnersRepository.CreateAsync(It.IsAny<Partner>()))
            .ReturnsAsync(partner);

        this.SetupSaveChangesMock(0);

        // Act
        var result = await this.createPartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenExceptionIsThrown()
    {
        // Arrange
        var createPartnerDto = new CreatePartnerDTO
        {
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
        };

        var command = new CreatePartnerCommand(createPartnerDto);
        const string errorMsg = "Database error";

        this.partnersRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Partner>()))
            .ThrowsAsync(new Exception(errorMsg));

        // Act
        var result = await this.createPartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);
        this.partnersRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Partner>()), Times.Once);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }
}