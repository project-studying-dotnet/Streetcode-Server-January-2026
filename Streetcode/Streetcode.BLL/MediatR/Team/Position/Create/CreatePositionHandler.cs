using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Team.Create
{
    public class CreatePositionHandler : IRequestHandler<CreatePositionQuery, Result<PositionDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public CreatePositionHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<PositionDTO>> Handle(CreatePositionQuery request, CancellationToken cancellationToken)
        {
            var newPosition = await _repository.PositionRepository.CreateAsync(
                new Positions
                {
                    Position = request.Position.Position
                });

            try
            {
                var success = await _repository.SaveChangesAsync() > 0;
                if (success)
                {
                    return Result.Ok(_mapper.Map<PositionDTO>(newPosition));
                }

                var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(Positions));
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }
    }
}