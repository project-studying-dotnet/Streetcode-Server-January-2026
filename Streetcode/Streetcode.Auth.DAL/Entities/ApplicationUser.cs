using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Streetcode.Auth.DAL.Entities
{
    [Table("users", Schema = "auth")]
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
