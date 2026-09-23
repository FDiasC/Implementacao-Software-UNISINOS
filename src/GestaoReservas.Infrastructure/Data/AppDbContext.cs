using GestaoReservas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoReservas.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Local> Locais => Set<Local>();

    public DbSet<Recurso> Recursos => Set<Recurso>();

    public DbSet<Reserva> Reservas => Set<Reserva>();

    public DbSet<ReservaRecurso> ReservaRecursos => Set<ReservaRecurso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
