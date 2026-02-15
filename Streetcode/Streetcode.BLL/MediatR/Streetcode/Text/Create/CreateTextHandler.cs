using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using EntityText = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create
{
    public class CreateTextHandler : IRequestHandler<CreateTextCommand, Result<TextDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public CreateTextHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<TextDTO>> Handle(CreateTextCommand command, CancellationToken cancellationToken)
        {
            var text = _mapper.Map<EntityText>(command.Text);

            var createdText = await _repositoryWrapper.TextRepository.CreateAsync(text);
            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(_mapper.Map<TextDTO>(createdText));
            }

            var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.Text));
            _logger.LogError(command, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
