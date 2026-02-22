using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;

public class GetTagByIdHandler : IRequestHandler<GetTagByIdQuery, Result<TagDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetTagByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TagDTO>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _repositoryWrapper.TagRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (tag is not null)
        {
            return Result.Ok(_mapper.Map<TagDTO>(tag));
        }

        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(
            nameof(DAL.Entities.AdditionalContent.Tag),
            request.Id);

        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
