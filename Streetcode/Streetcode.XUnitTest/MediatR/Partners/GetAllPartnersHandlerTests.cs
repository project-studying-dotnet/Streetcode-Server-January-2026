using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Partners;
using Streetcode.BLL.Mapping.Streetcode;
using Streetcode.BLL.MediatR.Partners.GetAll;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Partners;

public class GetAllPartnersHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly GetAllPartnersHandler getAllPartnersHandler;

    public GetAllPartnersHandlerTests()
    {
        this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        this.partnersRepositoryMock = new Mock<IPartnersRepository>();
        this.loggerMock = new Mock<ILoggerService>();

        this.repositoryWrapperMock
            .Setup(r => r.PartnersRepository)
            .Returns(this.partnersRepositoryMock.Object);

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new PartnerProfile());
            cfg.AddProfile(new PartnerSourceLinkProfile());
            cfg.AddProfile(new StreetcodeProfile());
        });
        this.mapper = new Mapper(configuration);

        this.getAllPartnersHandler = new GetAllPartnersHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    private void SetupGetAllPartners(List<Partner> partners)
    {
        this.partnersRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(partners);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessResult_WhenPartnersExist()
    {
        // Arrange
        var partners = new List<Partner>
        {
            new ()
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
            },
            new ()
            {
                Id = 2,
                IsKeyPartner = true,
                IsVisibleEverywhere = true,
                Title = "Test Title 2",
                Description = "Test Description 2",
                TargetUrl = "http://partner-url2.com",
                LogoId = 2,
                UrlTitle = "test-title 2",
                Streetcodes =
                [
                    new StreetcodeContent
                    {
                        Id = 3,
                        Title = "Test Streetcode 3",
                    },
                    new StreetcodeContent
                    {
                        Id = 4,
                        Title = "Test Streetcode 4",
                    },
                ],
                PartnerSourceLinks =
                [
                    new PartnerSourceLink
                    {
                        Id = 3,
                        LogoType = LogoType.YouTube,
                        TargetUrl = "http://sourcelink3-url.com",
                    },
                    new PartnerSourceLink
                    {
                        Id = 4,
                        LogoType = LogoType.Instagram,
                        TargetUrl = "http://sourcelink4-url.com",
                    },
                ],
            },
        };

        var expectedPartnerDtos = new List<PartnerDTO>
        {
            new ()
            {
                Id = 1,
                IsKeyPartner = false,
                IsVisibleEverywhere = false,
                Title = "Test Title",
                Description = "Test Description",
                TargetUrl = new UrlDTO
                {
                    Title = "test-title",
                    Href = "http://partner-url.com",
                },
                LogoId = 1,
                Streetcodes =
                [
                    new StreetcodeShortDTO
                    {
                        Id = 1,
                        Title = "Test Streetcode",
                    },
                    new StreetcodeShortDTO
                    {
                        Id = 2,
                        Title = "Test Streetcode 2",
                    },
                ],
                PartnerSourceLinks =
                [
                    new PartnerSourceLinkDTO
                    {
                        Id = 1,
                        LogoType = LogoTypeDTO.Twitter,
                        TargetUrl = new UrlDTO
                        {
                            Href = "http://sourcelink1-url.com",
                        },
                    },
                    new PartnerSourceLinkDTO
                    {
                        Id = 2,
                        LogoType = LogoTypeDTO.Facebook,
                        TargetUrl = new UrlDTO
                        {
                            Href = "http://sourcelink2-url.com",
                        },
                    },
                ],
            },
            new ()
            {
                Id = 2,
                IsKeyPartner = true,
                IsVisibleEverywhere = true,
                Title = "Test Title 2",
                Description = "Test Description 2",
                TargetUrl = new UrlDTO
                {
                    Title = "test-title 2",
                    Href = "http://partner-url2.com",
                },
                LogoId = 2,
                Streetcodes =
                [
                    new StreetcodeShortDTO
                    {
                        Id = 3,
                        Title = "Test Streetcode 3",
                    },
                    new StreetcodeShortDTO
                    {
                        Id = 4,
                        Title = "Test Streetcode 4",
                    },
                ],
                PartnerSourceLinks =
                [
                    new PartnerSourceLinkDTO
                    {
                        Id = 3,
                        LogoType = LogoTypeDTO.YouTube,
                        TargetUrl = new UrlDTO
                        {
                            Href = "http://sourcelink3-url.com",
                        },
                    },
                    new PartnerSourceLinkDTO
                    {
                        Id = 4,
                        LogoType = LogoTypeDTO.Instagram,
                        TargetUrl = new UrlDTO
                        {
                            Href = "http://sourcelink4-url.com",
                        },
                    },
                ],
            },
        };

        var query = new GetAllPartnersQuery();

        this.SetupGetAllPartners(partners);

        // Act
        var result = await this.getAllPartnersHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPartnerDtos.Count, result.Value.Count());
        result.Value.Should().BeEquivalentTo(expectedPartnerDtos);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenNoPartnersExist()
    {
        // Arrange
        var query = new GetAllPartnersQuery();
        var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(Partner));

        this.SetupGetAllPartners([]);

        // Act
        var result = await this.getAllPartnersHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);
    }
}