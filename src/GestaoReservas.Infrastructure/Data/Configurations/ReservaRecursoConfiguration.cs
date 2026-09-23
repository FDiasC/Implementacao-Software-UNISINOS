using GestaoReservas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoReservas.Infrastructure.Data.Configurations;

public class ReservaRecursoConfiguration : IEntityTypeConfiguration<ReservaRecurso>
{
    public void Configure(EntityTypeBuilder<ReservaRecurso> builder)
    {
        builder.ToTable("ReservaRecursos");

        builder.HasKey(rr => new { rr.ReservaId, rr.RecursoId });

        builder.HasOne(rr => rr.Reserva)
            .WithMany(r => r.ReservaRecursos)
            .HasForeignKey(rr => rr.ReservaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rr => rr.Recurso)
            .WithMany(r => r.ReservaRecursos)
            .HasForeignKey(rr => rr.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
