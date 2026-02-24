using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;

public class GetCoordinatesByStreetcodeIdHandler : IRequestHandler<GetCoordinatesByStreetcodeIdQuery, Result<IEnumerable<StreetcodeCoordinateDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetCoordinatesByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<StreetcodeCoordinateDTO>>> Handle(GetCoordinatesByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var coordinates = await _repositoryWrapper.StreetcodeCoordinateRepository
            .GetAllAsync(predicate: c => c.StreetcodeId == request.StreetcodeId);

        if (coordinates.Any())
        {
            return Result.Ok(_mapper.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates));
        }

        var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(nameof(StreetcodeCoordinate), request.StreetcodeId);
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
