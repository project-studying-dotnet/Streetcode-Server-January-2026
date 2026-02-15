using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Video;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Video.GetByStreetcodeId;

public class GetVideoByStreetcodeIdHandler : IRequestHandler<GetVideoByStreetcodeIdQuery, Result<VideoDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetVideoByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VideoDTO>> Handle(GetVideoByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var video = await _repositoryWrapper.VideoRepository
            .GetFirstOrDefaultAsync(video => video.StreetcodeId == request.StreetcodeId);
        if (video != null)
        {
            return new Result<VideoDTO>().WithValue(_mapper.Map<VideoDTO>(video));
        }

        var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
            nameof(DAL.Entities.Media.Video),
            request.StreetcodeId);

        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}