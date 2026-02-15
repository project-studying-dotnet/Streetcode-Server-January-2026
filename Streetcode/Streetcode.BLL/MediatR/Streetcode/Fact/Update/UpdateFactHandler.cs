using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

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
                var errorFactNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(DAL.Entities.Streetcode.TextContent.Fact),
                    request.Fact.Id);

                _logger.LogError(request, errorFactNotFoundMsg);
                return Result.Fail(new Error(errorFactNotFoundMsg));
            }

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
                var errorMsg = Messages.Error_FailedToUpdateEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.Fact));
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var updatedFactDTO = _mapper.Map<FactDTO>(fact);
            updatedFactDTO.ImageDescription = request.Fact.ImageDescription;

            return Result.Ok(updatedFactDTO);
        }
    }
}
