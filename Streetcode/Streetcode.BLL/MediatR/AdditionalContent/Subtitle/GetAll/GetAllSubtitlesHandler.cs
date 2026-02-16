using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;

public class GetAllSubtitlesHandler : IRequestHandler<GetAllSubtitlesQuery, Result<IEnumerable<SubtitleDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetAllSubtitlesHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<SubtitleDTO>>> Handle(GetAllSubtitlesQuery request, CancellationToken cancellationToken)
    {
        var subtitles = await _repositoryWrapper.SubtitleRepository.GetAllAsync();

        if (subtitles.Any())
        {
            return Result.Ok(_mapper.Map<IEnumerable<SubtitleDTO>>(subtitles));
        }

        var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.AdditionalContent.Subtitle));
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}