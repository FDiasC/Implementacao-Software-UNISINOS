using GestaoReservas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoReservas.Infrastructure.Data.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reservas", t => t.HasCheckConstraint(
            "CK_Reserva_DataFinal_MaiorIgual_DataInicial",
            "\"DataFinal\" > \"DataInicial\" OR (\"DataFinal\" = \"DataInicial\" AND \"HoraFinal\" > \"HoraInicial\")"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.DataInicial)
            .IsRequired();

        builder.Property(r => r.HoraInicial)
            .IsRequired();

        builder.Property(r => r.DataFinal)
            .IsRequired();

        builder.Property(r => r.HoraFinal)
            .IsRequired();

        builder.Property(r => r.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(r => r.Usuario)
            .WithMany(u => u.Reservas)
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Local)
            .WithMany(l => l.Reservas)
            .HasForeignKey(r => r.LocalId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
