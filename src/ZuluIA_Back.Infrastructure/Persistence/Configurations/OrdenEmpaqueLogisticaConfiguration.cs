using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Logistica;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenEmpaqueLogisticaConfiguration : IEntityTypeConfiguration<OrdenEmpaque>
{
    public void Configure(EntityTypeBuilder<OrdenEmpaque> builder)
    {
        builder.ToTable("ordenes_empaque_logistica");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SucursalTerceroId).HasColumnName("sucursal_tercero_id");
        builder.Property(x => x.VendedorId).HasColumnName("vendedor_id");
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id");
        builder.Property(x => x.TransportistaId).HasColumnName("transportista_id");
        builder.Property(x => x.AgenteId).HasColumnName("agente_id");
        builder.Property(x => x.TipoComprobanteId).HasColumnName("tipo_comprobante_id");
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id");
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id");
        builder.Property(x => x.Cotizacion).HasColumnName("cotizacion").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Prefijo).HasColumnName("prefijo");
        builder.Property(x => x.NroComprobante).HasColumnName("nro_comprobante");
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.FechaEmbarque).HasColumnName("fecha_embarque");
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.OrigenObservacion).HasColumnName("origen_observacion").HasMaxLength(500);
        builder.Property(x => x.DestinoObservacion).HasColumnName("destino_observacion").HasMaxLength(500);
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Anulada).HasColumnName("anulada").HasDefaultValue(false);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.Metadata.FindNavigation(nameof(OrdenEmpaque.Detalles))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Detalles)
            .WithOne()
            .HasForeignKey(x => x.OrdenEmpaqueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.Estado);
    }
}
