using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.AdditionalContent.Tag.Create
{
  public class CreateTagHandler : IRequestHandler<CreateTagCommand, Result<TagDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public CreateTagHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<TagDTO>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newTag = await _repositoryWrapper.TagRepository.CreateAsync(
                    new DAL.Entities.AdditionalContent.Tag
                    {
                        Title = request.Tag.Title
                    });

                var success = await _repositoryWrapper.SaveChangesAsync() > 0;
                if (success)
                {
                    return Result.Ok(_mapper.Map<TagDTO>(newTag));
                }

                var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(DAL.Entities.AdditionalContent.Tag));
                _logger.LogError(request, errorMsg);
                return Result.Fail<TagDTO>(errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.ToString());
                return Result.Fail(ex.ToString());
            }
        }
    }
}
