using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;

public class GetTextByStreetcodeIdHandler : IRequestHandler<GetTextByStreetcodeIdQuery, Result<TextDTO?>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ITextService _textService;
    private readonly ILoggerService _logger;

    public GetTextByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ITextService textService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _textService = textService;
        _logger = logger;
    }

    public async Task<Result<TextDTO?>> Handle(GetTextByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var text = await _repositoryWrapper.TextRepository
            .GetFirstOrDefaultAsync(text => text.StreetcodeId == request.StreetcodeId);

        if (text is null)
        {
            var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(
                nameof(DAL.Entities.Streetcode.TextContent.Text),
                request.StreetcodeId);

            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        text.TextContent = await _textService.AddTermsTag(text?.TextContent ?? string.Empty);

        return new Result<TextDTO?>().WithValue(_mapper.Map<TextDTO?>(text));
    }
}