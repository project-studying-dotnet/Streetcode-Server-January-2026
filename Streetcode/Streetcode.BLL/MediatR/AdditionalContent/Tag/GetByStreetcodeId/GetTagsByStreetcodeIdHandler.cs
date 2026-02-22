using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;

public class GetTagsByStreetcodeIdHandler : IRequestHandler<GetTagByStreetcodeIdQuery, Result<IEnumerable<StreetcodeTagDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetTagsByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<StreetcodeTagDTO>>> Handle(GetTagByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var tagsIndexed = await _repositoryWrapper.StreetcodeTagIndexRepository
            .GetAllAsync(
                t => t.StreetcodeId == request.StreetcodeId,
                include: q => q.Include(t => t.Tag));

        if (tagsIndexed.Any())
        {
            return Result.Ok(_mapper.Map<IEnumerable<StreetcodeTagDTO>>(tagsIndexed.OrderBy(ti => ti.Index)));
        }

        var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(DAL.Entities.AdditionalContent.Tag),
            request.StreetcodeId);

        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
