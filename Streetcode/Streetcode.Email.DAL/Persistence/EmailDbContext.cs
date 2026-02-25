using Microsoft.EntityFrameworkCore;
using Streetcode.Email.DAL.Entities;

namespace Streetcode.Email.DAL.Persistence
{
    public class EmailDbContext : DbContext
    {
        public EmailDbContext(DbContextOptions<EmailDbContext> options)
        : base(options)
        {
        }

        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
