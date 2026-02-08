using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.Newss;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class CreateNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly IMapper _mapper;
        private readonly CreateNewsHandler _handler;

        public CreateNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();

            var config = new MapperConfiguration(conf =>
            {
                conf.AddProfile(new NewsProfile());
                conf.AddProfile(new ImageProfile());
            });

            _mapper = config.CreateMapper();

            _handler = new CreateNewsHandler(
                _mapper,
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

            var command = new CreateNewsCommand(newsDto);

            _repositoryWrapperMock.Setup(r => r.NewsRepository.Create(
                It.IsAny<NewsEntity>())).Returns<NewsEntity>(x => x);

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

            var command = new CreateNewsCommand(newsDto);

            _repositoryWrapperMock.Setup(r => r.NewsRepository.Create(
                It.IsAny<NewsEntity>())).Returns<NewsEntity>(x => x);

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

            var res = await _handler.Handle(command, default);

            Assert.True(res.IsFailed);
            Assert.Equal("Cannot convert null to news", res.Errors.First().Message);
        }
    }
}
