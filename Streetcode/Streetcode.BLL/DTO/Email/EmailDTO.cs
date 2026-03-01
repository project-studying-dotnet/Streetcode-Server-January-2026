using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.DTO.Email
{
    public class EmailDTO
    {
        public EmailDTO()
        {
        }

        [MaxLength(80)]
        public string Email { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Message { get; set; }
    }
}
