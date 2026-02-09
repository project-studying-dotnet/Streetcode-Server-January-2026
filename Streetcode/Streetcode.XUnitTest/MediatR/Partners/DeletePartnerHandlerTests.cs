using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Partners;
using Streetcode.BLL.Mapping.Streetcode;
using Streetcode.BLL.MediatR.Partners.Delete;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Partners;

public class DeletePartnerHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<IPartnersRepository> partnersRepositoryMock;
    private readonly Mock<ILoggerService> loggerMock;
    private readonly IMapper mapper;
    private readonly DeletePartnerHandler deletePartnerHandler;

    public DeletePartnerHandlerTests()
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

        this.deletePartnerHandler = new DeletePartnerHandler(
            this.repositoryWrapperMock.Object,
            this.mapper,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsDeletedPartner_WhenPartnerExists()
    {
        // Arrange
        const int partnerId = 1;

        var partner = new Partner
        {
            Id = partnerId,
            IsKeyPartner = true,
            IsVisibleEverywhere = true,
            Title = "Test Title",
            Description = "Test Description",
            TargetUrl = "http://partner-url.com",
            LogoId = 1,
            UrlTitle = "test-title",
            PartnerSourceLinks = [],
            Streetcodes = [],
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
            PartnerSourceLinks = [],
            Streetcodes = [],
        };

        var command = new DeletePartnerCommand(partnerId);

        this.partnersRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(partner);

        this.repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await this.deletePartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Should().BeEquivalentTo(expectedPartnerDto);
    }

    [Fact]
    public async Task Handle_ReturnsFailedResult_WhenPartnerDoesNotExist()
    {
        // Arrange
        const int partnerId = 1;
        var command = new DeletePartnerCommand(partnerId);
        var errorMsg = $"Partner with id {partnerId} not found";

        this.partnersRepositoryMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await this.deletePartnerHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        Assert.Equal(errorMsg, result.Errors.First().Message);

        this.loggerMock.Verify(
            l => l.LogError(It.IsAny<object>(), errorMsg),
            Times.Once);
    }
}