using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AfipWsfeAuditConfiguration : IEntityTypeConfiguration<AfipWsfeAudit>
{
    public void Configure(EntityTypeBuilder<AfipWsfeAudit> builder)
    {
        builder.ToTable("afip_wsfe_audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.Operacion)
            .HasColumnName("operacion")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoOperacionAfipWsfe>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Exitoso).HasColumnName("exitoso").IsRequired();
        builder.Property(x => x.RequestPayload).HasColumnName("request_payload").IsRequired();
        builder.Property(x => x.ResponsePayload).HasColumnName("response_payload").IsRequired();
        builder.Property(x => x.MensajeError).HasColumnName("mensaje_error").HasMaxLength(2000);
        builder.Property(x => x.Cae).HasColumnName("cae").HasMaxLength(50);
        builder.Property(x => x.Caea).HasColumnName("caea").HasMaxLength(50);
        builder.Property(x => x.FechaOperacion).HasColumnName("fecha_operacion").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => x.Operacion);
        builder.HasIndex(x => x.FechaOperacion);
    }
}
