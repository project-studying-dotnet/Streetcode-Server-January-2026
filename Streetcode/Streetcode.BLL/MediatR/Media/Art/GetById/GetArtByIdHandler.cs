using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Art.GetById;

public class GetArtByIdHandler : IRequestHandler<GetArtByIdQuery, Result<ArtDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetArtByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ArtDTO>> Handle(GetArtByIdQuery request, CancellationToken cancellationToken)
    {
        var art = await _repositoryWrapper.ArtRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (art is not null)
        {
            return Result.Ok(_mapper.Map<ArtDTO>(art));
        }

        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.Media.Images.Art), request.Id);
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}