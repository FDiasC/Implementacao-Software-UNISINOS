using GestaoReservas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoReservas.Infrastructure.Data.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("Recursos", t => t.HasCheckConstraint(
            "CK_Recurso_DiasMaximos_MaiorIgual_DiasMinimos",
            "[DiasMaximosReserva] >= [DiasMinimosReserva]"));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Descricao)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.NumeroPatrimonio)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.NumeroPatrimonio)
            .IsUnique();

        builder.Property(r => r.DiasMinimosReserva)
            .IsRequired();

        builder.Property(r => r.DiasMaximosReserva)
            .IsRequired();

        builder.Property(r => r.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(r => r.Categoria)
            .WithMany(c => c.Recursos)
            .HasForeignKey(r => r.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
