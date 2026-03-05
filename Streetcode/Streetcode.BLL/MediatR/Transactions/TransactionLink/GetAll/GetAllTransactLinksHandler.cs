using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;

public class GetAllTransactLinksHandler : IRequestHandler<GetAllTransactLinksQuery, Result<IEnumerable<TransactLinkDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetAllTransactLinksHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TransactLinkDTO>>> Handle(GetAllTransactLinksQuery request, CancellationToken cancellationToken)
    {
        var transactLinks = await _repositoryWrapper.TransactLinksRepository.GetAllAsync();

        if (transactLinks?.Any() ?? false)
        {
            return Result.Ok(_mapper.Map<IEnumerable<TransactLinkDTO>>(transactLinks));
        }

        var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.Transactions.TransactionLink));
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}