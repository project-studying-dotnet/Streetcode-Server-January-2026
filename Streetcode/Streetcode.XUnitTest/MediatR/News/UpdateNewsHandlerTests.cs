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
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;
using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

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
        public async Task Handle_ShouldReturnFail_WhenDTOIsNull()
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
        public async Task Handle_ShouldReturnFail_WhenNewsWithIdNotFound()
        {
            // Arrange
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 0,
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    null,
                    false))
                .ReturnsAsync((NewsEntity)null);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(Messages.Error_EntityWithIdNotFound.Format(
                nameof(DAL.Entities.News.News), newsDto.Id));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenCouldntUpdateNews()
        {
            // Arrange
            var fakeBase = "fake_base_64";
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 1,
                Image = new ImageDTO
                {
                    Id = 1,
                },
            };

            var news = new NewsEntity
            {
                Id = 1,
                URL = "url",
                ImageId = 1,
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Image)null!);

            _blobServiceMock.Setup(s => s.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync(fakeBase);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            _repositoryWrapperMock.Verify(
                repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()),
                Times.Once());

            _repositoryWrapperMock.Verify(
                repo => repo.ImageRepository.Delete(It.IsAny<Image>()),
                Times.Never);

            res.Errors.Should().ContainSingle(Messages.Error_FailedToUpdateEntity.Format(
                nameof(DAL.Entities.News.News)));
        }

        [Fact]
        public async Task Handle_ShouldReturnNewsDtoWithImage_WhenNewImageExists()
        {
            // Arrange
            var fakeBase = "fake_base_64";
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
                Image = new ImageDTO
                {
                    Id = 2,
                },
            };

            var expectedNewsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
                Image = new ImageDTO
                {
                    Id = 2,
                    Base64 = fakeBase,
                },
            };

            var news = new NewsEntity
            {
                Id = 1,
                URL = "url",
                ImageId = 1,
                Image = new Image
                {
                    Id = 1,
                },
            };

            var newImage = new Image { Id = 2 };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(newImage);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((FactEntity)null!);

            _blobServiceMock.Setup(s => s.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync(fakeBase);

            _blobServiceMock.Setup(s => s.DeleteFileInStorage(It.IsAny<string>()));

            _repositoryWrapperMock
                .Setup(repo => repo.ImageRepository.Delete(It.IsAny<Image>()));

            _repositoryWrapperMock
                .Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));

            _repositoryWrapperMock
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            _repositoryWrapperMock.Verify(
                repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()),
                Times.Once());

            _repositoryWrapperMock.Verify(
                repo => repo.ImageRepository.Delete(It.IsAny<Image>()),
                Times.Once());

            _blobServiceMock.Verify(
                b => b.DeleteFileInStorage(It.IsAny<string>()),
                Times.Once());

            res.Value.Should().BeEquivalentTo(expectedNewsDto);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewImageNotExists()
        {
            // Arrange
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
                Image = new ImageDTO
                {
                    Id = 2,
                },
            };

            var news = new NewsEntity
            {
                Id = 1,
                URL = "url",
                ImageId = 1,
                Image = new Image
                {
                    Id = 1,
                },
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Image)null!);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(Messages.Error_EntityWithIdNotFound.Format(
                nameof(Image),
                newsDto.ImageId));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenNewImageExistsButBlobNotExists()
        {
            // Arrange
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
                Image = new ImageDTO
                {
                    Id = 2,
                },
            };

            var news = new NewsEntity
            {
                Id = 1,
                URL = "url",
                ImageId = 1,
                Image = new Image
                {
                    Id = 1,
                },
            };

            var newImage = new Image
            {
                Id = 2,
                BlobName = "BlobName",
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(newImage);

            _blobServiceMock.Setup(s => s.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsFailed.Should().BeTrue();
            res.Errors.Should().ContainSingle(Messages.Error_MediaBlobNotFound.Format(
                nameof(Image),
                newImage.BlobName));
        }

        [Fact]
        public async Task Handle_ShouldDeleteOldImage_WhenNewImageIsNullAndFactNotLinkedToImage()
        {
            // Arrange
            var fakeBase = "fake_base_64";
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
            };

            var expectedNewsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
            };

            var news = new NewsEntity
            {
                Id = 1,
                URL = "url",
                ImageId = 1,
                Image = new Image
                {
                    Id = 1,
                },
            };

            var newImage = new Image { Id = 2 };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(newImage);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((FactEntity)null!);

            _blobServiceMock.Setup(s => s.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync(fakeBase);

            _blobServiceMock.Setup(s => s.DeleteFileInStorage(It.IsAny<string>()));

            _repositoryWrapperMock
                .Setup(repo => repo.ImageRepository.Delete(It.IsAny<Image>()));

            _repositoryWrapperMock
                .Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));

            _repositoryWrapperMock
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            _repositoryWrapperMock.Verify(
                repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()),
                Times.Once());

            _repositoryWrapperMock.Verify(
                repo => repo.ImageRepository.Delete(It.IsAny<Image>()),
                Times.Once());

            _blobServiceMock.Verify(
                b => b.DeleteFileInStorage(It.IsAny<string>()),
                Times.Once());

            res.Value.Should().BeEquivalentTo(expectedNewsDto);
        }
        
        [Fact]
        public async Task Handle_ShouldNotDeleteOldImage_WhenNewImageIsNullAndFactLinkedToImage()
        {
            // Arrange
            var fakeBase = "fake_base_64";
            var newsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
            };

            var expectedNewsDto = new NewsDTO
            {
                Id = 1,
                URL = "url1",
                ImageId = 2,
            };

            var news = new NewsEntity
            {
                Id = 1,
                URL = "url",
                ImageId = 1,
                Image = new Image
                {
                    Id = 1,
                },
            };

            var newImage = new Image { Id = 2 };

            var fact = new FactEntity
            {
                Id = 1,
                ImageId = 1,
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(newImage);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(fact);

            _blobServiceMock.Setup(s => s.FindFileInStorageAsBase64(It.IsAny<string>()))
                .ReturnsAsync(fakeBase);

            _blobServiceMock.Setup(s => s.DeleteFileInStorage(It.IsAny<string>()));

            _repositoryWrapperMock
                .Setup(repo => repo.ImageRepository.Delete(It.IsAny<Image>()));

            _repositoryWrapperMock
                .Setup(repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()));

            _repositoryWrapperMock
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            var req = new UpdateNewsCommand(newsDto);

            // Act
            var res = await _handler.Handle(req, CancellationToken.None);

            // Assert
            res.IsSuccess.Should().BeTrue();
            _repositoryWrapperMock.Verify(
                repo => repo.NewsRepository.Update(It.IsAny<NewsEntity>()),
                Times.Once());

            _repositoryWrapperMock.Verify(
                repo => repo.ImageRepository.Delete(It.IsAny<Image>()),
                Times.Never);

            _blobServiceMock.Verify(
                b => b.DeleteFileInStorage(It.IsAny<string>()),
                Times.Never);

            res.Value.Should().BeEquivalentTo(expectedNewsDto);
        }
    }
}
