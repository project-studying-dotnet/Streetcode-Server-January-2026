using AutoMapper;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.DAL.Entities.Streetcode.Comments;

namespace Streetcode.BLL.Mapping.Streetcode.Comments
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.Name} {src.User.Surname}" : "Unknown User"))
                .ReverseMap();

            CreateMap<CreateCommentDTO, Comment>();
            CreateMap<UpdateCommentDTO, Comment>();
        }
    }
}
