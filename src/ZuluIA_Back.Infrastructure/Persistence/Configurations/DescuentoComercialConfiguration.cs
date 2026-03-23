using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class DescuentoComercialConfiguration : IEntityTypeConfiguration<DescuentoComercial>
{
    public void Configure(EntityTypeBuilder<DescuentoComercial> builder)
    {
        builder.ToTable("descuentos_comerciales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id")
               .IsRequired();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id")
               .IsRequired();

        builder.Property(x => x.FechaDesde)
               .HasColumnName("fecha_desde")
               .IsRequired();

        builder.Property(x => x.FechaHasta)
               .HasColumnName("fecha_hasta");

        builder.Property(x => x.Porcentaje)
               .HasColumnName("porcentaje")
               .HasPrecision(10, 4)
               .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.TerceroId, x.ItemId, x.FechaDesde });
    }
}
