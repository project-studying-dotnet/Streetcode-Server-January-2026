using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class CreateNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateNewsHandler _handler;

        public CreateNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreateNewsHandler(
                _mapperMock.Object,
                _repositoryWrapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCreatedNewsDTO_WhenValidRequest()
        {
            var newsDto = new NewsDTO
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now
            };

            var news = new NewsEntity
            {
                Title = "Test Title",
                Text = "Sample text",
                URL = "https://github.com/",
                CreationDate = DateTime.Now
            };

            var command = new CreateNewsCommand(newsDto);

            _mapperMock.Setup(m => m.Map<NewsEntity>(It.IsAny<NewsDTO>())).Returns(news);

            _mapperMock.Setup(m => m.Map<NewsDTO>(It.IsAny<NewsEntity>())).Returns(newsDto);

            _repositoryWrapperMock.Setup(r => r.NewsRepository.Create(
                It.IsAny<NewsEntity>())).Returns(news);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var res = await _handler.Handle(command, default);

            res.IsSuccess.Should().BeTrue();
            res.Value.Should().BeEquivalentTo(newsDto);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailCreating_WhenNewsDtoIsInvalidFormat()
        {
            var newsDto = new NewsDTO
            {
                Title = "",
                Text = "Sample text",
                URL = "invalid-url",
                CreationDate = DateTime.Now
            };

            var news = new NewsEntity
            {
                Title = "",
                Text = "Sample text",
                URL = "invalid-url",
                CreationDate = DateTime.Now
            };

            var command = new CreateNewsCommand(newsDto);

            _mapperMock.Setup(m => m.Map<NewsEntity>(It.IsAny<NewsDTO>())).Returns(news);

            _repositoryWrapperMock.Setup(r => r.NewsRepository.Create(
                It.IsAny<NewsEntity>())).Returns(news);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            var res = await _handler.Handle(command, default);

            Assert.True(res.IsFailed);
            Assert.Equal("Failed to create a news", res.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailConverting_WhenNewsDtoIsNull()
        {
            NewsDTO newsDto = null;
            var command = new CreateNewsCommand(newsDto);
            _mapperMock.Setup(m => m.Map<NewsEntity>(It.IsAny<NewsDTO>())).Returns((NewsEntity)null);

            var res = await _handler.Handle(command, default);

            Assert.True(res.IsFailed);
            Assert.Equal("Cannot convert null to news", res.Errors.First().Message);
        }
    }
}
