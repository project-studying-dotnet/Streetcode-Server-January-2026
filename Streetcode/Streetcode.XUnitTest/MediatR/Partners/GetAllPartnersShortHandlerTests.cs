using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Partners;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.BLL.MediatR.Partners.GetAllPartnersShort;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Partners;

public class GetAllPartnersShortHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly GetAllPartnersShortHandler getAllPartnersShortHandler;

    public GetAllPartnersShortHandlerTests()
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
        });
        this.mapper = new Mapper(configuration);

        this.getAllPartnersShortHandler = new GetAllPartnersShortHandler(
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
            },
        };

        var expectedPartnerDtos = new List<PartnerShortDTO>
        {
            new ()
            {
                Id = 1,
                Title = "Test Title",
            },
            new ()
            {
                Id = 2,
                Title = "Test Title 2",
            },
        };

        var query = new GetAllPartnersShortQuery();

        this.SetupGetAllPartners(partners);

        // Act
        var result = await this.getAllPartnersShortHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPartnerDtos.Count, result.Value.Count());
        result.Value.Should().BeEquivalentTo(expectedPartnerDtos);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenNoPartnersExist()
    {
        // Arrange
        var query = new GetAllPartnersShortQuery();
        const string errorMsg = "Cannot find any partners";
        this.SetupGetAllPartners([]);

        // Act
        var result = await this.getAllPartnersShortHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            x => x.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }
}