using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetById;

public class GetFactByIdHandler : IRequestHandler<GetFactByIdQuery, Result<FactDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetFactByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<FactDTO>> Handle(GetFactByIdQuery request, CancellationToken cancellationToken)
    {
        var fact = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(
            predicate: f => f.Id == request.Id,
            include: q => q
                .Include(f => f.Image)
                .ThenInclude(i => i.ImageDetails));

        if (fact is not null)
        {
            return Result.Ok(_mapper.Map<FactDTO>(fact));
        }

        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.Streetcode.TextContent.Fact), request.Id);
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}