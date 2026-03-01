using AutoMapper;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.DAL.Entities;

namespace Streetcode.Email.BLL.Mapping
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<Feedback, EmailDTO>().ReverseMap();
        }
    }
}
