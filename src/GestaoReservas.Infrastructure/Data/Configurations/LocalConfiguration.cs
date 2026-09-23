using GestaoReservas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoReservas.Infrastructure.Data.Configurations;

public class LocalConfiguration : IEntityTypeConfiguration<Local>
{
    public void Configure(EntityTypeBuilder<Local> builder)
    {
        builder.ToTable("Locais");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Sala)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Predio)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Capacidade)
            .IsRequired();

        builder.Property(l => l.Andar)
            .IsRequired();

        builder.Property(l => l.PermiteRecursos)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(l => l.Categoria)
            .WithMany(c => c.Locais)
            .HasForeignKey(l => l.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
