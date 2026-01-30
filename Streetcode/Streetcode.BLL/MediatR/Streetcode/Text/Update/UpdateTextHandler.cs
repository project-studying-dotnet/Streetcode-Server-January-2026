using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Entity.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Update
{
    public class UpdateTextHandler : IRequestHandler<UpdateTextCommand, Result<TextDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateTextHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<TextDTO>> Handle(UpdateTextCommand request, CancellationToken cancellationToken)
        {
            var text = await _repositoryWrapper.TextRepository
                .GetFirstOrDefaultAsync(x => x.Id == request.Text.StreetcodeId);

            if (text == null)
            {
                string errorMsg = $"No text found with Id {request.Text.StreetcodeId}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            text = _mapper.Map(request.Text, text);

            _repositoryWrapper.TextRepository.Update(text);
            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!successSave)
            {
                string errorMsg = "Error while saving changes to database";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var updatedTextDTO = _mapper.Map<TextDTO>(text);
            return Result.Ok(updatedTextDTO);
        }
    }
}
