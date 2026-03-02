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

        public DbSet<Email> Emails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Email>(entity =>
                {
                    entity.ToTable("Feedbacks");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                    entity.Property(e => e.Message).IsRequired();
                });
        }
    }
}
