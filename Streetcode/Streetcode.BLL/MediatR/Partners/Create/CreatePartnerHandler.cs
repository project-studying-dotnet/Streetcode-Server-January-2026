using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

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
            if (request.NewPartner.LogoId < 1)
            {
                var errorMsg = Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreatePartnerDTO.LogoId));
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var newPartner = _mapper.Map<Partner>(request.NewPartner);
            try
            {
                await HandleRelations(newPartner, request);
                await _repositoryWrapper.PartnersRepository.CreateAsync(newPartner);
                var result = await _repositoryWrapper.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result.Ok(_mapper.Map<PartnerDTO>(newPartner));
                }

                var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(Partner));
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
            if (request.NewPartner.Streetcodes is { Count: > 0 })
            {
                var streetcodeIds = request.NewPartner.Streetcodes
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

            if (request.NewPartner.PartnerSourceLinks is { Count: > 0 })
            {
                await _repositoryWrapper.PartnerSourceLinkRepository.CreateRangeAsync(newPartner.PartnerSourceLinks);
            }
        }
    }
}
