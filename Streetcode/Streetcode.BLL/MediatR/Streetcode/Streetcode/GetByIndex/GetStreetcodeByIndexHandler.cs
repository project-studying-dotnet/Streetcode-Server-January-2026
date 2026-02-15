using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByIndex;

public class GetStreetcodeByIndexHandler : IRequestHandler<GetStreetcodeByIndexQuery, Result<StreetcodeDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetStreetcodeByIndexHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StreetcodeDTO>> Handle(GetStreetcodeByIndexQuery request, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
            predicate: st => st.Index == request.Index,
            include: source => source.Include(l => l.Tags));

        if (streetcode is not null)
        {
            return Result.Ok(_mapper.Map<StreetcodeDTO>(streetcode));
        }

        var errorMsg = Messages.Error_StreetcodeWithIndexNotFound.Format(request.Index);
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}