using Microsoft.EntityFrameworkCore;
using Data.Entities;

namespace Data;

public class PetBnBDbContext : DbContext
{
    public PetBnBDbContext(DbContextOptions<PetBnBDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sitter> Sitters { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<PetType> PetTypes { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<SitterServiceType> SitterServiceTypes { get; set; }
    public DbSet<SitterPetType> SitterPetTypes { get; set; }
    public DbSet<SitterBadge> SitterBadges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite keys for many-to-many join tables
        modelBuilder.Entity<SitterServiceType>()
            .HasKey(e => new { e.SitterId, e.ServiceTypeId });
        modelBuilder.Entity<SitterPetType>()
            .HasKey(e => new { e.SitterId, e.PetTypeId });
        modelBuilder.Entity<SitterBadge>()
            .HasKey(e => new { e.SitterId, e.BadgeId });
    }
}
