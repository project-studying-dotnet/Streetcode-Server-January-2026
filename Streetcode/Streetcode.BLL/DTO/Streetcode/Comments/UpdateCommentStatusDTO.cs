using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Streetcode.Comments
{
    public class UpdateCommentStatusDTO
    {
        public int Id { get; set; }
        public CommentStatus Status { get; set; }
    }
}
