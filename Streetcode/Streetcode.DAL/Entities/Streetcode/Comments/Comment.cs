using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;

namespace Streetcode.DAL.Entities.Streetcode.Comments
{
    [Table("comments", Schema = "streetcode")]
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string TextContent { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int StreetcodeId { get; set; }

        public StreetcodeContent? Streetcode { get; set; }

        [Required]
        public string UserId { get; set; }

        public User? User { get; set; }

        public int? ParentId { get; set; }

        public Comment? Parent { get; set; }

        public List<Comment> Replies { get; set; } = new();

        [Required]
        public CommentStatus Status { get; set; } = CommentStatus.Pending;
    }
}
