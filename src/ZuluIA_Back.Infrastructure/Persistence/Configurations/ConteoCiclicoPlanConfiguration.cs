using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ConteoCiclicoPlanConfiguration : IEntityTypeConfiguration<ConteoCiclicoPlan>
{
    public void Configure(EntityTypeBuilder<ConteoCiclicoPlan> builder)
    {
        builder.ToTable("conteos_ciclicos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Deposito)
            .HasColumnName("deposito")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Zona)
            .HasColumnName("zona")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Frecuencia)
            .HasColumnName("frecuencia")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.ProximoConteo)
            .HasColumnName("proximo_conteo")
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.DivergenciaPct)
            .HasColumnName("divergencia_pct")
            .HasPrecision(9, 2)
            .IsRequired();

        builder.Property(x => x.Responsable)
            .HasColumnName("responsable")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Observacion)
            .HasColumnName("observacion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.NextStep)
            .HasColumnName("next_step")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ExecutionNote)
            .HasColumnName("execution_note")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.ProximoConteo);
    }
}
