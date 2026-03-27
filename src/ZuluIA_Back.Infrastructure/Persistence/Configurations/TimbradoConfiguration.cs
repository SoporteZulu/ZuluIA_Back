using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TimbradoConfiguration : IEntityTypeConfiguration<Timbrado>
{
    public void Configure(EntityTypeBuilder<Timbrado> builder)
    {
        builder.ToTable("timbrado");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.TipoComprobanteId).HasColumnName("tipo_comprobante_id").IsRequired();
        builder.Property(x => x.NroTimbrado).HasColumnName("nro_timbrado").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.NroComprobanteDesde).HasColumnName("nro_comprobante_desde");
        builder.Property(x => x.NroComprobanteHasta).HasColumnName("nro_comprobante_hasta");
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(x => new { x.SucursalId, x.PuntoFacturacionId, x.TipoComprobanteId, x.Activo });
    }
}
