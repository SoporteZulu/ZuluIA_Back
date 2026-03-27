using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ContratoDetalleConfiguration : IEntityTypeConfiguration<ContratoDetalle>
{
    public void Configure(EntityTypeBuilder<ContratoDetalle> builder)
    {
        builder.ToTable("contratos_detalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ContratoId).HasColumnName("contrato_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4);
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 4);
        builder.Property(x => x.PorcentajeIva).HasColumnName("porcentaje_iva").HasPrecision(8, 2);
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2);
        builder.Property(x => x.FechaDesde).HasColumnName("fecha_desde");
        builder.Property(x => x.FechaHasta).HasColumnName("fecha_hasta");
        builder.Property(x => x.FechaPrimeraFactura).HasColumnName("fecha_primera_factura");
        builder.Property(x => x.FrecuenciaMeses).HasColumnName("frecuencia_meses");
        builder.Property(x => x.Corte).HasColumnName("corte");
        builder.Property(x => x.Dominio).HasColumnName("dominio").HasMaxLength(50);
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(30);
        builder.Property(x => x.Anulado).HasColumnName("anulado").HasDefaultValue(false);

        builder.HasIndex(x => x.ContratoId);
    }
}
