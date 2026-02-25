using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Media.Audio.GetById;

public class GetAudioByIdHandler : IRequestHandler<GetAudioByIdQuery, Result<AudioDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetAudioByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<AudioDTO>> Handle(GetAudioByIdQuery request, CancellationToken cancellationToken)
    {
        var audio = await _repositoryWrapper.AudioRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (audio is null)
        {
            var errorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(DAL.Entities.Media.Audio), request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var audioDto = _mapper.Map<AudioDTO>(audio);

        var audioBase64 = await _blobService.FindFileInStorageAsBase64(audioDto.BlobName);

        if (audioBase64 is not null)
        {
            audioDto.Base64 = audioBase64;
            return Result.Ok(audioDto);
        }

        var errorNotFoundMsg = Messages.Error_MediaBlobNotFound.Format(
            nameof(DAL.Entities.Media.Audio),
            audioDto.BlobName);

        _logger.LogError(request, errorNotFoundMsg);
        return Result.Fail(new Error(errorNotFoundMsg));
    }
}