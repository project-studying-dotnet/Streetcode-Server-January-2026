using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update
{
    public class UpdateRelatedTermHandler : IRequestHandler<UpdateRelatedTermCommand, Result<RelatedTermDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public UpdateRelatedTermHandler(
            IMapper mapper,
            ILoggerService logger,
            IRepositoryWrapper repository)
        {
            _mapper = mapper;
            _logger = logger;
            _repository = repository;
        }

        public async Task<Result<RelatedTermDTO>> Handle(UpdateRelatedTermCommand request, CancellationToken cancellationToken)
        {
            var relatedTerm = await _repository.RelatedTermRepository.GetFirstOrDefaultAsync(
                x => x.Id == request.UpdateRelatedTerm.Id);

            if (relatedTerm == null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm),
                    request.UpdateRelatedTerm.Id);

                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            _mapper.Map(request.UpdateRelatedTerm, relatedTerm);
            _repository.RelatedTermRepository.Update(relatedTerm);
            var result = await _repository.SaveChangesAsync() > 0;
            if (result)
            {
                return Result.Ok(_mapper.Map<RelatedTermDTO>(relatedTerm));
            }

            var errorMsg = Messages.Error_FailedToUpdateEntity.Format(
                nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm));

            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
