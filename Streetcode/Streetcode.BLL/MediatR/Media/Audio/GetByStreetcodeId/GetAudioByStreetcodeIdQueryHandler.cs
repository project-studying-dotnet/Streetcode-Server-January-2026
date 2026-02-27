using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;

public class GetAudioByStreetcodeIdQueryHandler : IRequestHandler<GetAudioByStreetcodeIdQuery, Result<AudioDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetAudioByStreetcodeIdQueryHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<AudioDTO>> Handle(GetAudioByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var audio = await _repositoryWrapper.AudioRepository
            .GetFirstOrDefaultAsync(a => a.Streetcode.Id == request.StreetcodeId);

        if (audio == null)
        {
            var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
                nameof(AudioEntity),
                request.StreetcodeId);

            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var audioDto = _mapper.Map<AudioDTO>(audio);

        audioDto.Base64 = _blobService.FindFileInStorageAsBase64(audioDto.BlobName);

        return Result.Ok(audioDto);
    }
}