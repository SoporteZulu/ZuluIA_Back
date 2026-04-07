using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroTransporteConfiguration : IEntityTypeConfiguration<TerceroTransporte>
{
    public void Configure(EntityTypeBuilder<TerceroTransporte> builder)
    {
        builder.ToTable("terceros_transportes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(x => x.TerceroId)
            .HasColumnName("tercero_id")
            .IsRequired();

        builder.Property(x => x.TransportistaId)
            .HasColumnName("transportista_id");

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Servicio)
            .HasColumnName("servicio")
            .HasMaxLength(150);

        builder.Property(x => x.Zona)
            .HasColumnName("zona")
            .HasMaxLength(150);

        builder.Property(x => x.Frecuencia)
            .HasColumnName("frecuencia")
            .HasMaxLength(150);

        builder.Property(x => x.Observacion)
            .HasColumnName("observacion")
            .HasMaxLength(500);

        builder.Property(x => x.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();

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
            .HasDatabaseName("ix_terceros_transportes_tercero_orden");

        builder.HasIndex(x => new { x.TerceroId, x.Principal })
            .HasDatabaseName("ix_terceros_transportes_tercero_principal");

        builder.HasIndex(x => x.TransportistaId)
            .HasDatabaseName("ix_terceros_transportes_transportista_id");

        builder.HasOne<Tercero>()
            .WithMany()
            .HasForeignKey(x => x.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Transportista>()
            .WithMany()
            .HasForeignKey(x => x.TransportistaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
