using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode.TextContent;
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
                string errorMsg = $"Invalid image format: {imageResult.MimeType}. Only jpeg, png, webp are allowed";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newFact = _mapper.Map<DAL.Entities.Streetcode.TextContent.Fact>(request.Fact);

            if (newFact is null)
            {
                const string errorMsg = "Failed to map CreateFactDTO to Fact entity";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            newFact = await _repositoryWrapper.FactRepository.CreateAsync(newFact);
            await _repositoryWrapper.SaveChangesAsync();
            return Result.Ok(_mapper.Map<FactDTO>(newFact));
        }
    }
}
