using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Streetcode.Email.DAL.Entities;

[Table("feedback", Schema = "email")]
public class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(1000)]
        public string? Message { get; set; }
    }

