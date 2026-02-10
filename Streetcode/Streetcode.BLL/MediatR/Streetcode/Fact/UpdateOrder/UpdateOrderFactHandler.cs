using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public class UpdateOrderFactHandler : IRequestHandler<UpdateOrderFactCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateOrderFactHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(UpdateOrderFactCommand request, CancellationToken cancellationToken)
        {
            foreach (var factDTO in request.Facts)
            {
                var fact = await _repositoryWrapper.FactRepository
                    .GetFirstOrDefaultAsync(x => x.Id == factDTO.Id);

                if (fact != null)
                {
                    fact.Order = factDTO.Order;
                    _repositoryWrapper.FactRepository.Update(fact);
                }
                else
                {
                    _logger.LogWarning($"Fact with Id {factDTO.Id} not found while reordering");
                }
            }

            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!successSave)
            {
                string errorMsg = "Error while updating facts order";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            return Result.Ok(Unit.Value);
        }
    }
}
