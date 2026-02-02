using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public class UpdateFactHandler : IRequestHandler<UpdateFactCommand, Result<FactDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };

        public UpdateFactHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<FactDTO>> Handle(UpdateFactCommand request, CancellationToken cancellationToken)
        {
            var fact = await _repositoryWrapper.FactRepository
                .GetFirstOrDefaultAsync(x => x.Id == request.Fact.Id);

            if (fact == null)
            {
                string errorMsg = $"Cannot find a fact with Id: {request.Fact.Id}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
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

            fact = _mapper.Map(request.Fact, fact);

            if (request.Fact.ImageDescription != null)
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

            _repositoryWrapper.FactRepository.Update(fact);

            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!successSave)
            {
                string errorMsg = "Error while saving changes to database";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var updatedFactDTO = _mapper.Map<FactDTO>(fact);
            updatedFactDTO.ImageDescription = request.Fact.ImageDescription;

            return Result.Ok(updatedFactDTO);
        }
    }
}
