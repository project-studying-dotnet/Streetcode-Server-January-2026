using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.SortedByDateTime
{
    public record SortedByDateTimeQuery : IRequest<Result<List<NewsDTO>>>;
}
