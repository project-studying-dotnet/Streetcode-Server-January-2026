using AutoMapper;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Partners.Create;
using Streetcode.BLL.DTO.Partners.Update;
using Streetcode.DAL.Entities.Partners;

namespace Streetcode.BLL.Mapping.Partners;

public class PartnerSourceLinkProfile : Profile
{
    public PartnerSourceLinkProfile()
    {
        CreateMap<CreatePartnerSourceLinkDTO, PartnerSourceLink>();
        CreateMap<UpdatePartnerSourceLinkDTO, PartnerSourceLink>()
            .ForPath(
                dest => dest.Id,
                opt => opt.Ignore());
        CreateMap<PartnerSourceLink, PartnerSourceLinkDTO>()
            .ForPath(
                dto => dto.TargetUrl.Href,
                conf => conf.MapFrom(ol => ol.TargetUrl));
    }
}
