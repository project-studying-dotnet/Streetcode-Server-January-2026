using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.News;
using Streetcode.BLL.MediatR.News.Update;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatR.News
{
    public class UpdateNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly IMapper _mapper;
        private readonly UpdateNewsHandler _handler;

        public UpdateNewsHandlerTests()
        {
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();

            var config = new MapperConfiguration(conf =>
            {
                conf.AddProfile(new NewsProfile());
                conf.AddProfile(new ImageProfile());
            });

            _mapper = config.CreateMapper();

            _handler = new UpdateNewsHandler(
                _repositoryWrapperMock.Object,
                _mapper,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNoNews()
        {
            // Arrange
            var req = new UpdateNewsCommand(null);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(Messages.Error_ConvertNullToEntity.Format(
                nameof(DAL.Entities.News.News)));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenCouldntUpdateNews()
        {
            // Arrange
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 0,
            };

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                null,
                false))
                .ReturnsAsync((Image)null);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            _repositoryWrapperMock.Verify(
                repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()),
                Times.Once());

            res.Errors.Should().ContainSingle(Messages.Error_FailedToUpdateEntity.Format(
                nameof(DAL.Entities.News.News)));
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewsAndImageExist()
        {
            // Arrange
            var fakeBase = "fake_base64";

            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 0,
                Image = new ImageDTO(),
            };

            _blobServiceMock.Setup(bs => bs.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns(fakeBase);

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);


            // Assert
            res.IsSuccess.Should().BeTrue();
            _repositoryWrapperMock.Verify(
                repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()),
                Times.Once());

            res.Value.Image.Base64.Should().Be(fakeBase);
        }

        [Fact]
        public async Task Handle_ShouldDeleteOldImage_WhenNewImageIsNull_AndOldImageExists()
        {
            // Arrange
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 10,
                Image = null,
            };

            var oldImage = new Image { Id = 10 };

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                null,
                false))
                .ReturnsAsync(oldImage);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();

            _repositoryWrapperMock.Verify(
                repo => repo.ImageRepository.Delete(oldImage),
                Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()), Times.Once);
        }
    }
}
