using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.News.Create
{
    public class CreateNewsHandler : IRequestHandler<CreateNewsCommand, Result<NewsDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        public CreateNewsHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<NewsDTO>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
        {
            var newNews = _mapper.Map<DAL.Entities.News.News>(request.NewNews);

            if (newNews is null)
            {
                var errorNullMsg = Messages.Error_ConvertNullToEntity.Format(nameof(DAL.Entities.News.News));
                _logger.LogError(request, errorNullMsg);
                return Result.Fail(new Error(errorNullMsg));
            }

            if (newNews.ImageId == 0)
            {
                newNews.ImageId = null;
            }

            var entity = await _repositoryWrapper.NewsRepository.CreateAsync(newNews);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<NewsDTO>(entity));
            }

            var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(DAL.Entities.News.News));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
