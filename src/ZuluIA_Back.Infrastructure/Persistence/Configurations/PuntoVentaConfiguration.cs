using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.PuntoVenta;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TimbradoFiscalConfiguration : IEntityTypeConfiguration<TimbradoFiscal>
{
    public void Configure(EntityTypeBuilder<TimbradoFiscal> builder)
    {
        builder.ToTable("timbrados_fiscales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.NumeroTimbrado).HasColumnName("numero_timbrado").HasMaxLength(50).IsRequired();
        builder.Property(x => x.VigenciaDesde).HasColumnName("vigencia_desde").IsRequired();
        builder.Property(x => x.VigenciaHasta).HasColumnName("vigencia_hasta").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.NumeroTimbrado).IsUnique();
        builder.HasIndex(x => x.PuntoFacturacionId);
    }
}

public class OperacionPuntoVentaConfiguration : IEntityTypeConfiguration<OperacionPuntoVenta>
{
    public void Configure(EntityTypeBuilder<OperacionPuntoVenta> builder)
    {
        builder.ToTable("operaciones_punto_venta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.Canal)
            .HasColumnName("canal")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<CanalOperacionPuntoVenta>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.ReferenciaExterna).HasColumnName("referencia_externa").HasMaxLength(100);
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => x.Canal);
    }
}

public class SifenOperacionConfiguration : IEntityTypeConfiguration<SifenOperacion>
{
    public void Configure(EntityTypeBuilder<SifenOperacion> builder)
    {
        builder.ToTable("sifen_operaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.TimbradoFiscalId).HasColumnName("timbrado_fiscal_id");
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoIntegracionFiscalAlternativa>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");
        builder.Property(x => x.CodigoSeguridad).HasColumnName("codigo_seguridad").HasMaxLength(100);
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => x.TimbradoFiscalId);
    }
}

public class DeuceOperacionConfiguration : IEntityTypeConfiguration<DeuceOperacion>
{
    public void Configure(EntityTypeBuilder<DeuceOperacion> builder)
    {
        builder.ToTable("deuce_operaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoIntegracionFiscalAlternativa>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.ReferenciaExterna).HasColumnName("referencia_externa").HasMaxLength(100).IsRequired();
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload");
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload");
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => x.ReferenciaExterna).IsUnique();
    }
}
