using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;

namespace Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update
{
    public class UpdateHistoricalContextHandler : IRequestHandler<UpdateHistoricalContextCommand, Result<HistoricalContextDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateHistoricalContextHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<HistoricalContextDTO>> Handle(UpdateHistoricalContextCommand request, CancellationToken cancellationToken)
        {
            var existingContext = await _repositoryWrapper.HistoricalContextRepository
                    .GetFirstOrDefaultAsync(hc => hc.Id == request.HistoricalContext.Id);

            if (existingContext is null)
            {
                var errorMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(HistoricalContextEntity),
                    request.HistoricalContext.Id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (existingContext.Title != request.HistoricalContext.Title)
            {
                var duplicateTitle = await _repositoryWrapper.HistoricalContextRepository
                    .GetFirstOrDefaultAsync(hc => hc.Title == request.HistoricalContext.Title);

                if (duplicateTitle is not null)
                {
                    var errorMsg = Messages.Error_HistoricalContextTitleAlreadyExists;
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }
            }

            _mapper.Map(request.HistoricalContext, existingContext);
            _repositoryWrapper.HistoricalContextRepository.Update(existingContext);
            await _repositoryWrapper.SaveChangesAsync();

            return Result.Ok(_mapper.Map<HistoricalContextDTO>(existingContext));
        }
    }
}
