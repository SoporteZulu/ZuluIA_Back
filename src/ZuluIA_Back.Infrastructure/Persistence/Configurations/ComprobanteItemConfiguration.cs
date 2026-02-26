using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteItemConfiguration : IEntityTypeConfiguration<ComprobanteItem>
{
    public void Configure(EntityTypeBuilder<ComprobanteItem> builder)
    {
        builder.ToTable("comprobantes_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4);
        builder.Property(x => x.CantidadBonif).HasColumnName("cantidad_bonificada").HasPrecision(18, 4);
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 4);
        builder.Property(x => x.DescuentoPct).HasColumnName("descuento_pct").HasPrecision(5, 2);
        builder.Property(x => x.AlicuotaIvaId).HasColumnName("alicuota_iva_id").IsRequired();
        builder.Property(x => x.SubtotalNeto).HasColumnName("subtotal_neto").HasPrecision(18, 2);
        builder.Property(x => x.IvaImporte).HasColumnName("iva_importe").HasPrecision(18, 2);
        builder.Property(x => x.TotalLinea).HasColumnName("total_linea").HasPrecision(18, 2);
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id");
        builder.Property(x => x.Orden).HasColumnName("orden");

        builder.HasIndex(x => x.ComprobanteId);
    }
}