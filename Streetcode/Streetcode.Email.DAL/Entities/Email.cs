using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Streetcode.Email.DAL.Entities;

[Table("emails", Schema = "email")]
public class Email
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string? From { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(1000)]
        public string? Content { get; set; }
    }

