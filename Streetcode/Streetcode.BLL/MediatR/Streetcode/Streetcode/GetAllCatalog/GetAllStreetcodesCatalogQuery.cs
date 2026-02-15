using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.RelatedFigure;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAllCatalog
{
  public record GetAllStreetcodesCatalogQuery(int Page, int Count) : IRequest<Result<IEnumerable<RelatedFigureDTO>>>;
}
