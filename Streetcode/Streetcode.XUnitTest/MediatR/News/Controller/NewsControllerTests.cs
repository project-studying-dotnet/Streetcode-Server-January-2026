using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.News.Create;
using Streetcode.BLL.MediatR.News.Delete;
using Streetcode.BLL.MediatR.News.GetAll;
using Streetcode.BLL.MediatR.News.GetById;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;
using Streetcode.BLL.MediatR.News.SortedByDateTime;
using Streetcode.BLL.MediatR.News.Update;
using Streetcode.WebApi.Controllers.News;
using Xunit;
using Error = FluentResults.Error;

namespace Streetcode.XUnitTest.MediatR.News.Controller;

public class NewsControllerTests
{
    private readonly Mock<IMediator> mediatorMock;
    private readonly Mock<IServiceProvider> serviceProviderMock;
    private readonly NewsController controller;

    public NewsControllerTests()
    {
        this.mediatorMock = new Mock<IMediator>();
        this.serviceProviderMock = new Mock<IServiceProvider>();

        this.serviceProviderMock
            .Setup(x => x.GetService(typeof(IMediator)))
            .Returns(this.mediatorMock.Object);

        this.controller = new NewsController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = this.serviceProviderMock.Object,
                },
            },
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var newsList = new List<NewsDTO>
        {
            new ()
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now,
            },
        };

        var expectedResult = new OkObjectResult(newsList);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllNewsQuery>(), default))
            .ReturnsAsync(Result.Ok<IEnumerable<NewsDTO>>(newsList));

        // Act
        var result = await this.controller.GetAll();

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllNewsQuery>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.GetAll();

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var id = 1;

        var news = new NewsDTO
        {
            Id = id,
            Title = "Test Title",
            Text = "Sample text",
            URL = "https://github.com/",
            CreationDate = DateTime.Now,
        };

        var expectedResult = new OkObjectResult(news);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewsByIdQuery>(), default))
            .ReturnsAsync(Result.Ok(news));

        // Act
        var result = await this.controller.GetById(id);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var id = 1;
        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewsByIdQuery>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.GetById(id);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetByUrl_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var url = "https://github.com/";

        var news = new NewsDTO
        {
            Title = "Test Title",
            Text = "Sample text",
            URL = "https://github.com/",
            CreationDate = DateTime.Now,
        };

        var expectedResult = new OkObjectResult(news);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewsByUrlQuery>(), default))
            .ReturnsAsync(Result.Ok(news));

        // Act
        var result = await this.controller.GetByUrl(url);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetByUrl_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var url = "https://github.com/";
        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewsByUrlQuery>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.GetByUrl(url);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetNewsAndLinksByUrl_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var url = "https://github.com/";

        var newsWithURLs = new NewsDTOWithURLs
        {
            News = new NewsDTO
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now,
            },
            PrevNewsUrl = "prev-url",
            NextNewsUrl = "next-url",
            RandomNews = new RandomNewsDTO
            {
                Title = "Title",
                RandomNewsUrl = "random-url",
            },
        };

        var expectedResult = new OkObjectResult(newsWithURLs);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewsAndLinksByUrlQuery>(), default))
            .ReturnsAsync(Result.Ok(newsWithURLs));

        // Act
        var result = await this.controller.GetNewsAndLinksByUrl(url);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetNewsAndLinksByUrl_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var url = "https://github.com/";
        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewsAndLinksByUrlQuery>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.GetNewsAndLinksByUrl(url);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetSortedByDateTime_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var newsList = new List<NewsDTO>
        {
            new ()
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now.AddDays(-1),
            },
            new ()
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now,
            },
        };

        var expectedResult = new OkObjectResult(newsList);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<SortedByDateTimeQuery>(), default))
            .ReturnsAsync(Result.Ok(newsList));

        // Act
        var result = await this.controller.GetSortedByDateTime();

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetSortedByDateTime_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<SortedByDateTimeQuery>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.GetSortedByDateTime();

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var news = new NewsDTO
        {
            Id = 1,
            Title = "Test Title",
            Text = "Sample text",
            URL = "https://github.com/",
            CreationDate = DateTime.Now,
        };

        var expectedResult = new OkObjectResult(news);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateNewsCommand>(), default))
            .ReturnsAsync(Result.Ok(news));

        // Act
        var result = await this.controller.Create(news);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var news = new NewsDTO();

        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateNewsCommand>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.Create(news);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var news = new NewsDTO
        {
            Id = 1,
            Title = "Test Title",
            Text = "Sample text",
            URL = "https://github.com/",
            CreationDate = DateTime.Now,
        };

        var expectedResult = new OkObjectResult(news);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateNewsCommand>(), default))
            .ReturnsAsync(Result.Ok(news));

        // Act
        var result = await this.controller.Update(news);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var news = new NewsDTO();

        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateNewsCommand>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.Update(news);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var id = 1;

        var expectedResult = new OkObjectResult(Unit.Value);

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteNewsCommand>(), default))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await this.controller.Delete(id);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenFail()
    {
        // Arrange
        var id = 1;
        var errorMsg = "Some error";
        var expectedResult = new BadRequestObjectResult(new List<Error>() { new (errorMsg) });

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteNewsCommand>(), default))
            .ReturnsAsync(Result.Fail(errorMsg));

        // Act
        var result = await this.controller.Delete(id);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
        result.Should().BeEquivalentTo(expectedResult);
    }
}