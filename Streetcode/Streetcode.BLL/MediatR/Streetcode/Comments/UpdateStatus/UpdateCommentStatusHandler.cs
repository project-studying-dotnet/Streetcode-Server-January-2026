using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus
{
    public class UpdateCommentStatusHandler : IRequestHandler<UpdateCommentStatusCommand, Result<CommentDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateCommentStatusHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CommentDTO>> Handle(UpdateCommentStatusCommand request, CancellationToken cancellationToken)
        {
            var comment = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(c => c.Id == request.Comment.Id);

            if (comment is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), request.Dto.Id);
                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            // Оновлюємо статус
            comment.Status = request.Dto.Status;

            // Фіксуємо час, коли адмін перевірив коментар
            comment.UpdatedAt = DateTime.UtcNow;

            _repositoryWrapper.CommentRepository.Update(comment);
            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(_mapper.Map<CommentDTO>(comment));
            }

            var errorMsg = Messages.Error_FailedToUpdateEntity.Format(nameof(Comment));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
