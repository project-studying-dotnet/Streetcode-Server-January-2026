using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

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
                .GetFirstOrDefaultAsync(x => x.Id == request.Text.Id);

            if (text == null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(DAL.Entities.Streetcode.TextContent.Text),
                    request.Text.Id);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            text = _mapper.Map(request.Text, text);

            _repositoryWrapper.TextRepository.Update(text);
            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(_mapper.Map<TextDTO>(text));
            }

            var errorMsg = Messages.Error_FailedToUpdateEntity.Format(
                nameof(DAL.Entities.Streetcode.TextContent.Text));

            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
