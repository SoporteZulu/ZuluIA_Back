using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroVentanaCobranzaConfiguration : IEntityTypeConfiguration<TerceroVentanaCobranza>
{
    public void Configure(EntityTypeBuilder<TerceroVentanaCobranza> builder)
    {
        builder.ToTable("terceros_ventanas_cobranza");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(x => x.TerceroId)
            .HasColumnName("tercero_id")
            .IsRequired();

        builder.Property(x => x.Dia)
            .HasColumnName("dia")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Franja)
            .HasColumnName("franja")
            .HasMaxLength(100);

        builder.Property(x => x.Canal)
            .HasColumnName("canal")
            .HasMaxLength(100);

        builder.Property(x => x.Responsable)
            .HasColumnName("responsable")
            .HasMaxLength(150);

        builder.Property(x => x.Principal)
            .HasColumnName("principal")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.Orden)
            .HasColumnName("orden")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.TerceroId, x.Orden })
            .HasDatabaseName("ix_terceros_ventanas_cobranza_tercero_orden");

        builder.HasIndex(x => new { x.TerceroId, x.Principal })
            .HasDatabaseName("ix_terceros_ventanas_cobranza_tercero_principal");

        builder.HasOne<Tercero>()
            .WithMany()
            .HasForeignKey(x => x.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
