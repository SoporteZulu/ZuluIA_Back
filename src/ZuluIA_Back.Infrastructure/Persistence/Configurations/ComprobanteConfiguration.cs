using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("comprobantes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id");
        builder.Property(x => x.TipoComprobanteId).HasColumnName("tipo_comprobante_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.Cotizacion).HasColumnName("cotizacion").HasPrecision(18, 6).HasDefaultValue(1m);
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2);
        builder.Property(x => x.DescuentoImporte).HasColumnName("descuento_importe").HasPrecision(18, 2);
        builder.Property(x => x.NetoGravado).HasColumnName("neto_gravado").HasPrecision(18, 2);
        builder.Property(x => x.NetoNoGravado).HasColumnName("neto_no_gravado").HasPrecision(18, 2);
        builder.Property(x => x.IvaRi).HasColumnName("iva_ri").HasPrecision(18, 2);
        builder.Property(x => x.IvaRni).HasColumnName("iva_rni").HasPrecision(18, 2);
        builder.Property(x => x.Percepciones).HasColumnName("percepciones").HasPrecision(18, 2);
        builder.Property(x => x.Retenciones).HasColumnName("retenciones").HasPrecision(18, 2);
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2);
        builder.Property(x => x.Saldo).HasColumnName("saldo").HasPrecision(18, 2);
        builder.Property(x => x.Cae).HasColumnName("cae").HasMaxLength(20);
        builder.Property(x => x.FechaVtoCae).HasColumnName("fecha_vto_cae");
        builder.Property(x => x.QrData).HasColumnName("qr_data");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString(),
                   v => Enum.Parse<EstadoComprobante>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.OwnsOne(x => x.Numero, n =>
        {
            n.Property(p => p.Prefijo).HasColumnName("prefijo").IsRequired();
            n.Property(p => p.Numero).HasColumnName("numero").IsRequired();
        });

        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey(x => x.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SucursalId, x.TipoComprobanteId });
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.Estado);
    }
}