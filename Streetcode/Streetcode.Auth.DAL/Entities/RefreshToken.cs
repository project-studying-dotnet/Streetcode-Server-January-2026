using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.Auth.DAL.Entities
{
    [Table("refreshTokens", Schema = "auth")]
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Token { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public bool Revoked { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
