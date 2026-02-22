using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.GetById
{
    public record GetNewsByIdQuery(int Id) : IRequest<Result<NewsDTO>>;
}
