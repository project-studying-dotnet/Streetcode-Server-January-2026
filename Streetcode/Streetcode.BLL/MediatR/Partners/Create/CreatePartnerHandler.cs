using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Partners.Create
{
    public class CreatePartnerHandler : IRequestHandler<CreatePartnerCommand, Result<PartnerDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public CreatePartnerHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PartnerDTO>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
        {
            // Move validation to a separate validator class if it becomes more complex
            if (request.newPartner.LogoId < 1)
            {
                const string errorMsg = "LogoId is required and must be greater than zero.";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var newPartner = _mapper.Map<Partner>(request.newPartner);
            try
            {
                await HandleRelations(newPartner, request);
                await _repositoryWrapper.PartnersRepository.CreateAsync(newPartner);
                var result = await _repositoryWrapper.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result.Ok(_mapper.Map<PartnerDTO>(newPartner));
                }

                const string errorMsg = "Failed to create a new Partner.";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }

        private async Task HandleRelations(Partner newPartner, CreatePartnerCommand request)
        {
            if (request.newPartner.Streetcodes is { Count: > 0 })
            {
                var streetcodeIds = request.newPartner.Streetcodes
                    .Select(s => s.Id)
                    .ToList();

                var streetcodes = await _repositoryWrapper.StreetcodeRepository
                    .GetAllAsync(s => streetcodeIds.Contains(s.Id), trackEntities: true);

                foreach (var streetcode in streetcodes)
                {
                    _repositoryWrapper.StreetcodeRepository.Attach(streetcode);
                }

                newPartner.Streetcodes = streetcodes.ToList();
            }

            if (request.newPartner.PartnerSourceLinks is { Count: > 0 })
            {
                await _repositoryWrapper.PartnerSourceLinkRepository.CreateRangeAsync(newPartner.PartnerSourceLinks);
            }
        }
    }
}
