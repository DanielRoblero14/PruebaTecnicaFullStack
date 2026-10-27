using Microsoft.EntityFrameworkCore;
using PruebaDanielRoblero.Models;

namespace PruebaDanielRoblero.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Empleado> Empleados => Set<Empleado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Departamento>()
            .HasIndex(d => d.Codigo)
            .IsUnique();

        modelBuilder.Entity<Empleado>()
            .Property(e => e.Codigo)
            .HasMaxLength(30)
            .IsRequired();

        modelBuilder.Entity<Empleado>()
            .HasIndex(e => e.Codigo)
            .IsUnique();

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Departamento)
            .WithMany()
            .HasForeignKey(e => e.DepartamentoId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
