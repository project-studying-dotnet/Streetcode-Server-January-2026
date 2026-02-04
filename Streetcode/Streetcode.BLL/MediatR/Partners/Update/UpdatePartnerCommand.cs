using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;

namespace Streetcode.BLL.MediatR.Partners.Update
{
  public record UpdatePartnerCommand(UpdatePartnerDTO Partner) : IRequest<Result<PartnerDTO>>;
}
