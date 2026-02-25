using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Streetcode.Email.DAL.Entities;

[Table("feedback", Schema = "email")]
public class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string? Message { get; set; }
    }

