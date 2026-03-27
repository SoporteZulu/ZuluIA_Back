using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("contratos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SucursalTerceroId).HasColumnName("sucursal_tercero_id");
        builder.Property(x => x.VendedorId).HasColumnName("vendedor_id");
        builder.Property(x => x.TipoComprobanteId).HasColumnName("tipo_comprobante_id");
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id");
        builder.Property(x => x.CondicionVentaId).HasColumnName("condicion_venta_id");
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id");
        builder.Property(x => x.Cotizacion).HasColumnName("cotizacion").HasPrecision(18, 6);
        builder.Property(x => x.FechaDesde).HasColumnName("fecha_desde");
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.FechaInicioFacturacion).HasColumnName("fecha_inicio_facturacion");
        builder.Property(x => x.PeriodoMeses).HasColumnName("periodo_meses");
        builder.Property(x => x.Duracion).HasColumnName("duracion");
        builder.Property(x => x.CuotasRestantes).HasColumnName("cuotas_restantes");
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(30);
        builder.Property(x => x.Anulado).HasColumnName("anulado").HasDefaultValue(false);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(1000);
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Detalles)
               .WithOne()
               .HasForeignKey(d => d.ContratoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => new { x.Anulado, x.Estado });
    }
}
