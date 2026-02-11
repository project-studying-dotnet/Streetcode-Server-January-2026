using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;

public class GetFactByStreetcodeIdHandler : IRequestHandler<GetFactByStreetcodeIdQuery, Result<IEnumerable<FactDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetFactByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<FactDTO>>> Handle(GetFactByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var facts = await _repositoryWrapper.FactRepository.GetAllAsync(
            predicate: f => f.StreetcodeId == request.StreetcodeId,
            include: q => q.Include(f => f.Image).ThenInclude(i => i.ImageDetails));

        if (facts.Any())
        {
            var sortedFacts = facts.OrderByDescending(f => f.Order);
            return Result.Ok(_mapper.Map<IEnumerable<FactDTO>>(sortedFacts));
        }

        var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(DAL.Entities.Streetcode.TextContent.Fact),
            request.StreetcodeId);

        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}