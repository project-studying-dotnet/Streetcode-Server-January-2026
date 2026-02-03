using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;

public class GetPartnersByStreetcodeIdHandler : IRequestHandler<GetPartnersByStreetcodeIdQuery, Result<IEnumerable<PartnerDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetPartnersByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PartnerDTO>>> Handle(GetPartnersByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var partners = await _repositoryWrapper.PartnersRepository
            .GetAllAsync(
                predicate: p => p.Streetcodes.Any(sc => sc.Id == request.StreetcodeId) || p.IsVisibleEverywhere,
                include: p => p.Include(pl => pl.PartnerSourceLinks));

        if (partners.Any())
        {
            return Result.Ok(value: _mapper.Map<IEnumerable<PartnerDTO>>(partners));
        }

        var errorMsg = $"Cannot find a partners by a streetcode id: {request.StreetcodeId}";
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
