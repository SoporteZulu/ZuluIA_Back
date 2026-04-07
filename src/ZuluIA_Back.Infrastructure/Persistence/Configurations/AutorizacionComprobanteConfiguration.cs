using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AutorizacionComprobanteConfiguration : IEntityTypeConfiguration<AutorizacionComprobante>
{
    public void Configure(EntityTypeBuilder<AutorizacionComprobante> builder)
    {
        builder.ToTable("autorizacion_comprobante");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TipoOperacion).HasColumnName("tipo_operacion").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(500);
        builder.Property(x => x.AutorizadoPor).HasColumnName("autorizado_por");
        builder.Property(x => x.FechaResolucion).HasColumnName("fecha_resolucion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => new { x.SucursalId, x.Estado });
    }
}

public class HabilitacionComprobanteConfiguration : IEntityTypeConfiguration<HabilitacionComprobante>
{
    public void Configure(EntityTypeBuilder<HabilitacionComprobante> builder)
    {
        builder.ToTable("habilitacion_comprobante");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.MotivoBloqueo).HasColumnName("motivo_bloqueo").HasMaxLength(500);
        builder.Property(x => x.ObservacionHabilitacion).HasColumnName("observacion_habilitacion").HasMaxLength(500);
        builder.Property(x => x.HabilitadoPor).HasColumnName("habilitado_por");
        builder.Property(x => x.FechaHabilitacion).HasColumnName("fecha_habilitacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => new { x.SucursalId, x.Estado });
    }
}