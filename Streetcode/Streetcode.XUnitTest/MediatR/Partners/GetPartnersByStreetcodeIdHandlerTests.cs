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
using Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Partners;

public class GetPartnersByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly GetPartnersByStreetcodeIdHandler handler;

    public GetPartnersByStreetcodeIdHandlerTests()
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

        this.handler = new GetPartnersByStreetcodeIdHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPartners_WhenPartnersWithStreetcodesExist()
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
                PartnerSourceLinks =
                [
                    new PartnerSourceLink
                    {
                        Id = 1,
                        LogoType = LogoType.Twitter,
                        TargetUrl = "http://sourcelink1-url.com",
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
                Streetcodes = [],
                PartnerSourceLinks =
                [
                    new PartnerSourceLink
                    {
                        Id = 2,
                        LogoType = LogoType.YouTube,
                        TargetUrl = "http://sourcelink2-url.com",
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
                Streetcodes = [],
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
                Streetcodes = [],
                PartnerSourceLinks =
                [
                    new PartnerSourceLinkDTO
                    {
                        Id = 2,
                        LogoType = LogoTypeDTO.YouTube,
                        TargetUrl = new UrlDTO
                        {
                            Href = "http://sourcelink2-url.com",
                        },
                    },
                ],
            },
        };

        this.partnersRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>,
                    IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(partners);

        var query = new GetPartnersByStreetcodeIdQuery(1);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPartnerDtos.Count, result.Value.Count());
        result.Value.Should().BeEquivalentTo(expectedPartnerDtos);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenNoPartnersWithStreetcodesExist()
    {
        // Arrange
        const int streetcodeId = 1;
        var errorMsg = $"Cannot find a partners by a streetcode id: {streetcodeId}";

        this.partnersRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>,
                    IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<Partner>());

        var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await this.handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }
}