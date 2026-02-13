using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetById;

public class GetStreetcodeByIdHandler : IRequestHandler<GetStreetcodeByIdQuery, Result<StreetcodeDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheService _cacheService;

    public GetStreetcodeByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger, ICacheService cacheService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<StreetcodeDTO>> Handle(GetStreetcodeByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"Streetcode_{request.Id}";

        var cachedStreetcode = await _cacheService.GetAsync<StreetcodeDTO>(cacheKey, cancellationToken);
        if (cachedStreetcode is not null)
        {
            return Result.Ok(cachedStreetcode);
        }

        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
            predicate: st => st.Id == request.Id);

        if (streetcode is null)
        {
            var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(StreetcodeContent), request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var tagIndexed = await _repositoryWrapper.StreetcodeTagIndexRepository
            .GetAllAsync(
                t => t.StreetcodeId == request.Id,
                q => q.Include(ti => ti.Tag));
        var streetcodeDto = _mapper.Map<StreetcodeDTO>(streetcode);
        streetcodeDto.Tags = _mapper.Map<List<StreetcodeTagDTO>>(tagIndexed);

        await _cacheService.SetAsync(cacheKey, streetcodeDto, TimeSpan.FromHours(1));

        return Result.Ok(streetcodeDto);
    }
}