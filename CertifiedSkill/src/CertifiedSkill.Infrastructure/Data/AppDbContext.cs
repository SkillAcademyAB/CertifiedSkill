using CertifiedSkill.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CertifiedSkill.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Consent> Consents => Set<Consent>();
    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasIndex(p => p.NationalIdHash)
            .IsUnique();

        modelBuilder.Entity<Person>()
            .HasMany(p => p.Consents)
            .WithOne(c => c.Person)
            .HasForeignKey(c => c.PersonId);

        modelBuilder.Entity<Person>()
            .HasMany(p => p.ExternalIdentities)
            .WithOne(e => e.Person)
            .HasForeignKey(e => e.PersonId);
    }
}