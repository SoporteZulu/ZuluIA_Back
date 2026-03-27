using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroSucursalEntregaConfiguration : IEntityTypeConfiguration<TerceroSucursalEntrega>
{
    public void Configure(EntityTypeBuilder<TerceroSucursalEntrega> builder)
    {
        builder.ToTable("terceros_sucursales_entrega");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(x => x.TerceroId)
            .HasColumnName("tercero_id")
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Direccion)
            .HasColumnName("direccion")
            .HasMaxLength(300);

        builder.Property(x => x.Localidad)
            .HasColumnName("localidad")
            .HasMaxLength(150);

        builder.Property(x => x.Responsable)
            .HasColumnName("responsable")
            .HasMaxLength(150);

        builder.Property(x => x.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(50);

        builder.Property(x => x.Horario)
            .HasColumnName("horario")
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
            .HasDatabaseName("ix_terceros_sucursales_entrega_tercero_orden");

        builder.HasIndex(x => new { x.TerceroId, x.Principal })
            .HasDatabaseName("ix_terceros_sucursales_entrega_tercero_principal");

        builder.HasOne<Tercero>()
            .WithMany()
            .HasForeignKey(x => x.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
