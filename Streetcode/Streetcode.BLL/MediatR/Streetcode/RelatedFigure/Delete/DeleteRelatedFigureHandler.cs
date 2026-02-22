using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedFigure.Delete;

public class DeleteRelatedFigureHandler : IRequestHandler<DeleteRelatedFigureCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public DeleteRelatedFigureHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteRelatedFigureCommand request, CancellationToken cancellationToken)
    {
        var relation = await _repositoryWrapper.RelatedFigureRepository.GetFirstOrDefaultAsync(
            rel =>
                rel.ObserverId == request.ObserverId &&
                rel.TargetId == request.TargetId);

        if (relation is null)
        {
            var errorRelationMsg = Messages.ErrorStreetcodeRelationError.Format(request.ObserverId, request.TargetId);
            _logger.LogError(request, errorRelationMsg);
            return Result.Fail(new Error(errorRelationMsg));
        }

        _repositoryWrapper.RelatedFigureRepository.Delete(relation);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (resultIsSuccess)
        {
            return Result.Ok(Unit.Value);
        }

        var errorMsg = Messages.Error_FailedToDeleteEntity.Format(nameof(DAL.Entities.Streetcode.RelatedFigure));
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
