using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using EntityHistoricalContext = Streetcode.DAL.Entities.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create
{
    public class CreateHistoricalContextHandler : IRequestHandler<CreateHistoricalContextCommand, Result<HistoricalContextDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public CreateHistoricalContextHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<HistoricalContextDTO>> Handle(CreateHistoricalContextCommand request, CancellationToken cancellationToken)
        {
            var existingContext = await _repositoryWrapper.HistoricalContextRepository
                    .GetFirstOrDefaultAsync(hc => hc.Title == request.HistoricalContext.Title);

            if (existingContext is not null)
            {
                var errorMsg = Messages.Error_HistoricalContextTitleAlreadyExists;
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newContext = _mapper.Map<EntityHistoricalContext>(request.HistoricalContext);
            newContext = await _repositoryWrapper.HistoricalContextRepository.CreateAsync(newContext);
            await _repositoryWrapper.SaveChangesAsync();

            return Result.Ok(_mapper.Map<HistoricalContextDTO>(newContext));
        }
    }
}
