using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;

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
            var streetcodeResult = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(s => s.Id == request.Fact.StreetcodeId);

            if (streetcodeResult is null)
            {
                const string errorMsg = "Streetcode with the specified id was not found";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var imageResult = await _repositoryWrapper.ImageRepository
                .GetFirstOrDefaultAsync(img => img.Id == request.Fact.ImageId);

            if (imageResult is null)
            {
                string errorMsg = "Image with the specified id was not found";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (imageResult.MimeType != null && !AllowedImageTypes.Contains(imageResult.MimeType))
            {
                string errorMsg = $"Invalid image format: {imageResult.MimeType}. Only jpeg, png, jpg or webp are allowed";
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
