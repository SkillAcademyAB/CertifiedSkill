using CertifiedSkill.Data.Participant;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<Certificate> Certificates => Set<Certificate>();
        public DbSet<ParticipantMagicLinkToken> ParticipantMagicLinkTokens => Set<ParticipantMagicLinkToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.DisplayName).HasMaxLength(256).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            builder.Entity<Certificate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CourseName).HasMaxLength(512).IsRequired();
                entity.Property(e => e.CourseVersion).HasMaxLength(128).IsRequired();
                entity.Property(e => e.IssuedAt).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.HasOne(e => e.Person)
                    .WithMany(p => p.Certificates)
                    .HasForeignKey(e => e.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ParticipantMagicLinkToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.TokenHash).HasMaxLength(88).IsRequired();
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
            });
        }
    }
}

