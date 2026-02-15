using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Partners.Update
{
    public class UpdatePartnerHandler : IRequestHandler<UpdatePartnerCommand, Result<PartnerDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public UpdatePartnerHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PartnerDTO>> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
        {
            // Move validation to a separate validator class if it becomes more complex
            if (request.Partner.LogoId < 1)
            {
                var errorMsg = Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(Partner.LogoId));
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            try
            {
                var partner = await _repositoryWrapper.PartnersRepository
                    .GetFirstOrDefaultAsync(
                        p => p.Id == request.Partner.Id,
                        x => x
                            .Include(p => p.PartnerSourceLinks)
                            .Include(p => p.Streetcodes),
                        trackEntities: true);

                if (partner == null)
                {
                    var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Partner), request.Partner.Id);
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(new Error(errorMsg));
                }

                _mapper.Map(request.Partner, partner);

                await HandleSourceLinkRelations(partner, request);
                await HandleStreetcodeRelations(partner, request);

                _repositoryWrapper.PartnersRepository.Update(partner);
                var result = await _repositoryWrapper.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result.Ok(_mapper.Map<PartnerDTO>(partner));
                }

                var resultErrorMsg = Messages.Error_FailedToUpdateEntity.Format(nameof(Partner));
                _logger.LogError(request, resultErrorMsg);
                return Result.Fail(new Error(resultErrorMsg));
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }

        private async Task HandleSourceLinkRelations(Partner partner, UpdatePartnerCommand request)
        {
            // Update existing PartnerSourceLinks
            var requestPartnerSourceLinkIds = request.Partner.PartnerSourceLinks
                .Select(psl => psl.Id)
                .ToList();

            var existingPartnerSourceLinks = partner.PartnerSourceLinks
                .Where(psl => requestPartnerSourceLinkIds.Contains(psl.Id))
                .ToList();

            foreach (var existingPartnerSourceLink in existingPartnerSourceLinks)
            {
                var partnerSourceLinkDto = request.Partner.PartnerSourceLinks
                    .First(psl => psl.Id == existingPartnerSourceLink.Id);
                _mapper.Map(partnerSourceLinkDto, existingPartnerSourceLink);
            }

            // Add new PartnerSourceLinks
            var existingPartnerSourceLinkIds = existingPartnerSourceLinks
                .Select(psl => psl.Id)
                .ToList();

            var partnerSourceLinkDtosToAdd = request.Partner.PartnerSourceLinks
                .Where(psl => !existingPartnerSourceLinkIds.Contains(psl.Id))
                .ToList();

            var partnerSourceLinksToAdd = _mapper
                .Map<IEnumerable<PartnerSourceLink>>(partnerSourceLinkDtosToAdd)
                .ToList();

            if (partnerSourceLinksToAdd.Count != 0)
            {
                await _repositoryWrapper.PartnerSourceLinkRepository.CreateRangeAsync(partnerSourceLinksToAdd);
            }

            // Combine existing and new PartnerSourceLinks. Deleted links are automatically removed due to the absence in this combined list.
            partner.PartnerSourceLinks = existingPartnerSourceLinks
                .Union(partnerSourceLinksToAdd)
                .ToList();
        }

        private async Task HandleStreetcodeRelations(Partner partner, UpdatePartnerCommand request)
        {
            var streetcodeIds = request.Partner.Streetcodes
                .Select(s => s.Id)
                .ToList();

            var streetcodes = await _repositoryWrapper.StreetcodeRepository
                .GetAllAsync(
                    s => streetcodeIds.Contains(s.Id),
                    trackEntities: true);

            partner.Streetcodes = streetcodes.ToList();
        }
    }
}
