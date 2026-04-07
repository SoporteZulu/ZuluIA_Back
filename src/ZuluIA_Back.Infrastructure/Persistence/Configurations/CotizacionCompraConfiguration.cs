using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CotizacionCompraConfiguration : IEntityTypeConfiguration<CotizacionCompra>
{
    public void Configure(EntityTypeBuilder<CotizacionCompra> builder)
    {
        builder.ToTable("cotizaciones_compra");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.RequisicionId).HasColumnName("requisicion_id");
        builder.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 4);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoCotizacionCompra>(v, true))
               .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.CotizacionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.ProveedorId);
    }
}

public class CotizacionCompraItemConfiguration : IEntityTypeConfiguration<CotizacionCompraItem>
{
    public void Configure(EntityTypeBuilder<CotizacionCompraItem> builder)
    {
        builder.ToTable("cotizaciones_compra_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CotizacionId).HasColumnName("cotizacion_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 4).IsRequired();
        builder.HasIndex(x => x.CotizacionId);
    }
}
