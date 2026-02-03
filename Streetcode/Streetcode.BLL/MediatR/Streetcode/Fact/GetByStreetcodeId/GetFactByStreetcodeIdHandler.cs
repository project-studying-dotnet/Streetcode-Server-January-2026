using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

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

        var sortedFacts = facts.OrderByDescending(f => f.Order);

        return Result.Ok(_mapper.Map<IEnumerable<FactDTO>>(sortedFacts));
    }
}