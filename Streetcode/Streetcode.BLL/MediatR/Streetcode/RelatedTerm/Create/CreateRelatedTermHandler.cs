using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create
{
    public class CreateRelatedTermHandler : IRequestHandler<CreateRelatedTermCommand, Result<RelatedTermDTO>>
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public CreateRelatedTermHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<RelatedTermDTO>> Handle(CreateRelatedTermCommand request, CancellationToken cancellationToken)
        {
            var relatedTerm = _mapper.Map<DAL.Entities.Streetcode.TextContent.RelatedTerm>(request.RelatedTerm);

            if (relatedTerm is null)
            {
                var errorConvertMsg =
                    Messages.Error_ConvertNullToEntity.Format(nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm));

                _logger.LogError(request, errorConvertMsg);
                return Result.Fail(new Error(errorConvertMsg));
            }

            var existingTerms = await _repository.RelatedTermRepository
                .GetAllAsync(rt => rt.TermId == request.RelatedTerm.TermId && rt.Word == request.RelatedTerm.Word);

            if (existingTerms.Any())
            {
                var errorWordDefinitionMsg = Messages.Error_WordDefinitionExists;
                _logger.LogError(request, errorWordDefinitionMsg);
                return Result.Fail(new Error(errorWordDefinitionMsg));
            }

            var createdRelatedTerm = await _repository.RelatedTermRepository.CreateAsync(relatedTerm);

            var isSuccessResult = await _repository.SaveChangesAsync() > 0;

            if (isSuccessResult)
            {
                return _mapper.Map<RelatedTermDTO>(createdRelatedTerm);
            }

            var errorMsg = Messages.Error_FailedToCreateEntity
                .Format(nameof(DAL.Entities.Streetcode.TextContent.RelatedTerm));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
