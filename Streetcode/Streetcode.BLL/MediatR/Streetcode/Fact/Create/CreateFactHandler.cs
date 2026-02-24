using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactHandler : IRequestHandler<CreateFactCommand, Result<FactDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };

        public CreateFactHandler(
            IMapper mapper,
            IRepositoryWrapper repositoryWrapper,
            ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<FactDTO>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
        {
            var imageResult = await _repositoryWrapper.ImageRepository
                .GetFirstOrDefaultAsync(img => img.Id == request.Fact.ImageId);

            if (imageResult is null)
            {
                var errorImageNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Image), request.Fact.ImageId);
                _logger.LogError(request, errorImageNotFoundMsg);
                return Result.Fail(errorImageNotFoundMsg);
            }

            if (imageResult.MimeType != null && !AllowedImageTypes.Contains(imageResult.MimeType))
            {
                var allowedTypes = string.Join(",", AllowedImageTypes);
                var errorMsg = Messages.Error_InvalidImageFormat.Format(imageResult.MimeType, allowedTypes);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newFact = _mapper.Map<DAL.Entities.Streetcode.TextContent.Fact>(request.Fact);

            if (!string.IsNullOrEmpty(request.Fact.ImageDescription))
            {
                var existingDetails = await _repositoryWrapper.ImageDetailsRepository
                    .GetFirstOrDefaultAsync(x => x.ImageId == request.Fact.ImageId);

                if (existingDetails != null)
                {
                    existingDetails.Title = request.Fact.ImageDescription;
                    _repositoryWrapper.ImageDetailsRepository.Update(existingDetails);
                }
                else
                {
                    var newDetails = new ImageDetails
                    {
                        ImageId = request.Fact.ImageId,
                        Title = request.Fact.ImageDescription
                    };
                    await _repositoryWrapper.ImageDetailsRepository.CreateAsync(newDetails);
                }
            }

            newFact = await _repositoryWrapper.FactRepository.CreateAsync(newFact);
            await _repositoryWrapper.SaveChangesAsync();

            var createdDto = _mapper.Map<FactDTO>(newFact);
            createdDto.ImageDescription = request.Fact.ImageDescription;

            return Result.Ok(createdDto);
        }
    }
}
