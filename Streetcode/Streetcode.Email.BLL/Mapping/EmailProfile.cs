using AutoMapper;
using Streetcode.Email.BLL.DTO;
using EmailEntity = Streetcode.Email.DAL.Entities.Email;

namespace Streetcode.Email.BLL.Mapping
{
    public class EmailProfile : Profile
    {
        public EmailProfile()
        {
            CreateMap<EmailEntity, EmailDTO>().ReverseMap();
        }
    }
}
