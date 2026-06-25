using EventosVivos_Api.Models;
using EventosVivos_Api.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos_Api.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venue> Venues { get; set; }
    public DbSet<EstadoEvento> EstadosEventos { get; set; }
    public DbSet<EstadoReserva> EstadosReservas { get; set; }
    public DbSet<TipoEvento> TiposEventos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("AspNetUsers", "Security");
            entity.Property(user => user.FullName).HasMaxLength(200);
            entity.Property(user => user.IsActive).HasDefaultValue(true);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("AspNetRoles", "Security");
            entity.Property(role => role.Description).HasMaxLength(250);
            entity.Property(role => role.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<UserRole>().ToTable("AspNetUserRoles", "Security");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "Security");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "Security");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "Security");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "Security");

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Correo)
            .IsUnique();
    }
}
