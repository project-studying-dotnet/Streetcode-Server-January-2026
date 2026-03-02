using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Delete
{
    public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteCommentHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
        {
            var comment = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(t => t.Id == command.Id);

            if (comment is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(Comment), command.Id);

                _logger.LogError(command, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            if (comment.UserId != command.UserId)
            {
                var errorAuthMsg = Messages.Error_UserNotCommentOwner;
                _logger.LogError(command, errorAuthMsg);
                return Result.Fail(new Error(errorAuthMsg));
            }

            _repositoryWrapper.CommentRepository.Delete(comment);

            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(Unit.Value);
            }

            var errorMsg = Messages.Error_FailedToDeleteEntity.Format(nameof(Comment));
            _logger.LogError(command, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
