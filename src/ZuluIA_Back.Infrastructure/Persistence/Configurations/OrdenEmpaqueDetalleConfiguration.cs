using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Logistica;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenEmpaqueDetalleConfiguration : IEntityTypeConfiguration<OrdenEmpaqueDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenEmpaqueDetalle> builder)
    {
        builder.ToTable("ordenes_empaque_detalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.OrdenEmpaqueId).HasColumnName("orden_empaque_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4);
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 4);
        builder.Property(x => x.PorcentajeIva).HasColumnName("porcentaje_iva").HasPrecision(8, 2);
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);

        builder.HasIndex(x => x.OrdenEmpaqueId);
    }
}
