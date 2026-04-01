using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteItemConfiguration : IEntityTypeConfiguration<ComprobanteItem>
{
    public void Configure(EntityTypeBuilder<ComprobanteItem> builder)
    {
        builder.ToTable("comprobantes_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id").IsRequired();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id").IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(300).IsRequired();

        builder.Property(x => x.Cantidad)
               .HasColumnName("cantidad")
               .HasPrecision(18, 4).IsRequired();

        // Alias para compatibilidad y claridad en la base de datos
        builder.Property(x => x.CantidadBonificada)
               .HasColumnName("cantidad_bonificada")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.PrecioUnitario)
               .HasColumnName("precio_unitario")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.DescuentoPct)
               .HasColumnName("descuento_pct")
               .HasPrecision(8, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.AlicuotaIvaId)
               .HasColumnName("alicuota_iva_id").IsRequired();

        builder.Property(x => x.PorcentajeIva)
               .HasColumnName("porcentaje_iva")
               .HasPrecision(8, 4);

        builder.Property(x => x.SubtotalNeto)
               .HasColumnName("subtotal_neto")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.IvaImporte)
               .HasColumnName("iva_importe")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.TotalLinea)
               .HasColumnName("total_linea")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.DepositoId)
               .HasColumnName("deposito_id");

        builder.Property(x => x.Orden)
               .HasColumnName("orden")
               .HasDefaultValue((short)0);

        builder.Property(x => x.EsGravado)
               .HasColumnName("es_gravado")
               .HasDefaultValue(true);

        // Campos extendidos para paridad con zuluApp
        builder.Property(x => x.Lote)
               .HasColumnName("lote")
               .HasMaxLength(100);

        builder.Property(x => x.Serie)
               .HasColumnName("serie")
               .HasMaxLength(100);

        builder.Property(x => x.FechaVencimiento)
               .HasColumnName("fecha_vencimiento");

        builder.Property(x => x.UnidadMedidaId)
               .HasColumnName("unidad_medida_id");

        builder.Property(x => x.ObservacionRenglon)
               .HasColumnName("observacion_renglon")
               .HasMaxLength(500);

        builder.Property(x => x.PrecioListaOriginal)
               .HasColumnName("precio_lista_original")
               .HasPrecision(18, 4);

        builder.Property(x => x.ComisionVendedorRenglon)
               .HasColumnName("comision_vendedor_renglon")
               .HasPrecision(18, 4);

        builder.Property(x => x.ComprobanteItemOrigenId)
               .HasColumnName("comprobante_item_origen_id");

        // Campos específicos para notas de débito/crédito (referencias al documento origen)
        builder.Property(x => x.CantidadDocumentoOrigen)
                 .HasColumnName("cantidad_documento_origen")
                 .HasPrecision(18, 4);

        builder.Property(x => x.PrecioDocumentoOrigen)
                 .HasColumnName("precio_documento_origen")
                 .HasPrecision(18, 4);

        // Campos de pedido (cumplimiento)
        builder.Property(x => x.CantidadEntregada)
               .HasColumnName("cantidad_entregada")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.CantidadPendiente)
               .HasColumnName("cantidad_pendiente")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.EstadoEntrega)
               .HasColumnName("estado_entrega_item")
               .HasConversion(
                   v => v.HasValue ? (int)v.Value : (int?)null,
                   v => v.HasValue ? (EstadoEntregaItem)v.Value : null);

        builder.Property(x => x.EsAtrasado)
               .HasColumnName("es_atrasado")
               .HasDefaultValue(false);

        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.ComprobanteItemOrigenId);
        builder.HasIndex(x => x.EstadoEntrega)
               .HasFilter("estado_entrega_item IS NOT NULL");
        builder.HasIndex(x => x.EsAtrasado)
               .HasFilter("es_atrasado = true");
        builder.HasIndex(x => new { x.ComprobanteId, x.EstadoEntrega });
    }
}
