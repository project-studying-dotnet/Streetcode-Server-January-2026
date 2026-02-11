using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Partners.Update;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Partners;
using Streetcode.BLL.Mapping.Streetcode;
using Streetcode.BLL.MediatR.Partners.Update;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Partners;

public class UpdatePartnerHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<IPartnerSourceLinkRepository> partnerSourceLinkRepositoryMock;
    private readonly Mock<IStreetcodeRepository> streetcodeRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly UpdatePartnerHandler updatePartnerHandler;

    public UpdatePartnerHandlerTests()
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

        this.updatePartnerHandler = new UpdatePartnerHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    private void SetupSaveChangesMock(int result)
    {
        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Handle_ReturnsUpdatedPartner_WhenUpdateSuccessful()
    {
        // Arrange
        var updatePartnerDto = new UpdatePartnerDTO
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title Updated",
            Description = "Test Description Updated",
            TargetUrl = "http://partner-url-updated.com",
            LogoId = 2,
            UrlTitle = "test-title-updated",
            Streetcodes =
            [
                new StreetcodeShortDTO
                {
                    Id = 1,
                    Title = "Test Streetcode",
                },
                new StreetcodeShortDTO
                {
                    Id = 3,
                    Title = "Test Streetcode 3",
                },
            ],
            PartnerSourceLinks =
            [
                new UpdatePartnerSourceLinkDTO
                {
                    Id = 1,
                    LogoType = LogoTypeDTO.Instagram,
                    TargetUrl = "http://sourcelink1-url-updated.com",
                },
                new UpdatePartnerSourceLinkDTO
                {
                    Id = 2343,
                    LogoType = LogoTypeDTO.Facebook,
                    TargetUrl = "http://sourcelink3-url.com",
                },
            ],
        };

        var streetcodes = new List<StreetcodeContent>
        {
            new ()
            {
                Id = 1,
                Title = "Test Streetcode",
            },

            new ()
            {
                Id = 3,
                Title = "Test Streetcode 3",
            },
        };

        var partner = new Partner
        {
            Id = 1,
            IsKeyPartner = false,
            IsVisibleEverywhere = false,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
            Streetcodes =
            [
                new StreetcodeContent
                {
                    Id = 1,
                    Title = "Test Streetcode",
                },

                new StreetcodeContent
                {
                    Id = 2,
                    Title = "Test Streetcode 2",
                },
            ],
            PartnerSourceLinks =
            [
                new PartnerSourceLink
                {
                    Id = 1,
                    LogoType = LogoType.Twitter,
                    TargetUrl = "http://sourcelink1-url.com",
                },
                new PartnerSourceLink
                {
                    Id = 2,
                    LogoType = LogoType.Facebook,
                    TargetUrl = "http://sourcelink2-url.com",
                },
            ],
        };

        var expectedPartnerDto = new PartnerDTO
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title Updated",
            Description = "Test Description Updated",
            TargetUrl = new UrlDTO
            {
                Title = "test-title-updated",
                Href = "http://partner-url-updated.com",
            },
            LogoId = 2,
            Streetcodes =
            [
                new StreetcodeShortDTO
                {
                    Id = 1,
                    Title = "Test Streetcode",
                },
                new StreetcodeShortDTO
                {
                    Id = 3,
                    Title = "Test Streetcode 3",
                },
            ],
            PartnerSourceLinks =
            [
                new PartnerSourceLinkDTO
                {
                    Id = 1,
                    LogoType = LogoTypeDTO.Instagram,
                    TargetUrl = new UrlDTO
                    {
                        Href = "http://sourcelink1-url-updated.com",
                    },
                },
                new PartnerSourceLinkDTO
                {
                    Id = 3,
                    LogoType = LogoTypeDTO.Facebook,
                    TargetUrl = new UrlDTO
                    {
                        Href = "http://sourcelink3-url.com",
                    },
                },
            ],
        };

        var command = new UpdatePartnerCommand(updatePartnerDto);

        this.partnersRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(partner);

        this.streetcodeRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(streetcodes);

        this.partnerSourceLinkRepositoryMock
            .Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<PartnerSourceLink>>()))
            .Callback((IEnumerable<PartnerSourceLink> links) =>
            {
                links.First().Id = 3;
            })
            .Returns(Task.CompletedTask);

        this.partnersRepositoryMock
            .Setup(r => r.Update(It.IsAny<Partner>()))
            .Returns((EntityEntry<Partner>)null!);

        this.SetupSaveChangesMock(1);

        // Act
        var result = await this.updatePartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Should().BeEquivalentTo(expectedPartnerDto);
    }

    [Fact]
    public async Task Update_ClearsLinksAndStreetcodes_WhenEmptyListsProvided()
    {
        // Arrange
        var updatePartnerDto = new UpdatePartnerDTO
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title Updated",
            Description = "Test Description Updated",
            TargetUrl = "http://partner-url-updated.com",
            LogoId = 2,
            UrlTitle = "test-title-updated",
            Streetcodes = [],
            PartnerSourceLinks = [],
        };

        var partner = new Partner
        {
            Id = 1,
            IsKeyPartner = false,
            IsVisibleEverywhere = false,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
            Streetcodes =
            [
                new StreetcodeContent
                {
                    Id = 1,
                    Title = "Test Streetcode",
                },

                new StreetcodeContent
                {
                    Id = 2,
                    Title = "Test Streetcode 2",
                },
            ],
            PartnerSourceLinks =
            [
                new PartnerSourceLink
                {
                    Id = 1,
                    LogoType = LogoType.Twitter,
                    TargetUrl = "http://sourcelink1-url.com",
                },
                new PartnerSourceLink
                {
                    Id = 2,
                    LogoType = LogoType.Facebook,
                    TargetUrl = "http://sourcelink2-url.com",
                },
            ],
        };

        var expectedPartnerDto = new PartnerDTO()
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title Updated",
            Description = "Test Description Updated",
            TargetUrl = new UrlDTO
            {
                Title = "test-title-updated",
                Href = "http://partner-url-updated.com",
            },
            LogoId = 2,
            Streetcodes = [],
            PartnerSourceLinks = [],
        };

        var command = new UpdatePartnerCommand(updatePartnerDto);

        this.partnersRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(partner);

        this.streetcodeRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync([]);

        this.partnersRepositoryMock
            .Setup(r => r.Update(It.IsAny<Partner>()))
            .Returns((EntityEntry<Partner>)null!);

        this.SetupSaveChangesMock(1);

        // Act
        var result = await this.updatePartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Should().BeEquivalentTo(expectedPartnerDto);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenLogoIdIsLessThanOne()
    {
        // Arrange
        var updatePartnerDto = new UpdatePartnerDTO
        {
            Id = 1,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 0,
            UrlTitle = "test-title",
        };

        var command = new UpdatePartnerCommand(updatePartnerDto);
        var errorMsg = Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdatePartnerDTO.LogoId));

        // Act
        var result = await this.updatePartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenPartnerNotFound()
    {
        // Arrange
        var updatePartnerDto = new UpdatePartnerDTO
        {
            Id = 0,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
        };

        var command = new UpdatePartnerCommand(updatePartnerDto);
        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Partner), updatePartnerDto.Id);

        this.partnersRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await this.updatePartnerHandler.Handle(command, CancellationToken.None);

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
        var updatePartnerDto = new UpdatePartnerDTO
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

        var command = new UpdatePartnerCommand(updatePartnerDto);
        const string errorMsg = "Database error";

        this.partnersRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ThrowsAsync(new Exception(errorMsg));

        // Act
        var result = await this.updatePartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }
}