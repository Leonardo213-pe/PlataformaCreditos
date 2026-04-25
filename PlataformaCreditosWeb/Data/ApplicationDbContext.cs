using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaCreditosWeb.Models;

namespace PlataformaCreditosWeb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // IngresosMensuales > 0
        builder.Entity<Cliente>().ToTable(tb =>
            tb.HasCheckConstraint(
                "CK_Cliente_Ingresos",
                "\"IngresosMensuales\" > 0"));

        // MontoSolicitado > 0
        builder.Entity<SolicitudCredito>().ToTable(tb =>
            tb.HasCheckConstraint(
                "CK_Solicitud_Monto",
                "\"MontoSolicitado\" > 0"));

        // Un usuario = un cliente
        builder.Entity<Cliente>()
            .HasIndex(c => c.UsuarioId)
            .IsUnique();

        // Precisión decimal para SQLite
        builder.Entity<Cliente>()
            .Property(c => c.IngresosMensuales)
            .HasColumnType("decimal(18,2)");

        builder.Entity<SolicitudCredito>()
            .Property(s => s.MontoSolicitado)
            .HasColumnType("decimal(18,2)");
    }
}