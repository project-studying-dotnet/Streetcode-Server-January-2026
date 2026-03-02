using Microsoft.EntityFrameworkCore;
using EmailEntity = Streetcode.Email.DAL.Entities.Email;

namespace Streetcode.Email.DAL.Persistence
{
    public class EmailDbContext : DbContext
    {
        public EmailDbContext(DbContextOptions<EmailDbContext> options)
        : base(options)
        {
        }

        public DbSet<EmailEntity> Emails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<EmailEntity>(entity =>
                {
                    entity.ToTable("Emails");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.From).IsRequired().HasMaxLength(256);
                    entity.Property(e => e.Content).IsRequired();
                });
        }
    }
}
