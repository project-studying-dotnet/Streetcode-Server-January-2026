using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetParsed
{
    public class GetParsedTextAdminPreviewHandler : IRequestHandler<GetParsedTextForAdminPreviewCommand, Result<string>>
    {
        private readonly ITextService _textService;

        public GetParsedTextAdminPreviewHandler(ITextService textService)
        {
            _textService = textService;
        }

        public async Task<Result<string>> Handle(GetParsedTextForAdminPreviewCommand request, CancellationToken cancellationToken)
        {
            var parsedText = await _textService.AddTermsTag(request.TextToParse);
            return Result.Ok(parsedText);
        }
    }
}
