using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PuntoFacturacionConfiguration : IEntityTypeConfiguration<PuntoFacturacion>
{
    public void Configure(EntityTypeBuilder<PuntoFacturacion> builder)
    {
        builder.ToTable("puntos_facturacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.TipoId)
               .HasColumnName("tipo_id")
               .IsRequired();

        builder.Property(x => x.Numero)
               .HasColumnName("numero")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200);

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.SucursalId, x.Numero }).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}