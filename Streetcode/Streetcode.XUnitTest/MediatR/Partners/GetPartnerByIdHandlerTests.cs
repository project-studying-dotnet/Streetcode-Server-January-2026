using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Partners;
using Streetcode.BLL.Mapping.Streetcode;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Partners;

public class GetPartnerByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly GetPartnerByIdHandler getPartnerByIdHandler;

    public GetPartnerByIdHandlerTests()
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

        this.getPartnerByIdHandler = new GetPartnerByIdHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPartner_WhenPartnerExists()
    {
        // Arrange
        const int partnerId = 1;

        var partner = new Partner
        {
            Id = partnerId,
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
        };

        var query = new GetPartnerByIdQuery(partnerId);

        this.partnersRepositoryMock
            .Setup(r => r.GetSingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
                It.IsAny<System.Func<System.Linq.IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(partner);

        // Act
        var result = await this.getPartnerByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Should().BeEquivalentTo(expectedPartnerDto);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenPartnerNotExists()
    {
        // Arrange
        const int partnerId = 0;
        var query = new GetPartnerByIdQuery(partnerId);
        var errorMsg = $"Cannot find any partner with corresponding id: {partnerId}";

        this.partnersRepositoryMock
            .Setup(r => r.GetSingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Partner, bool>>>(),
                It.IsAny<System.Func<System.Linq.IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await this.getPartnerByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }
}