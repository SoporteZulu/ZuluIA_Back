using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AfipWsfeConfiguracionConfiguration : IEntityTypeConfiguration<AfipWsfeConfiguracion>
{
    public void Configure(EntityTypeBuilder<AfipWsfeConfiguracion> builder)
    {
        builder.ToTable("afip_wsfe_configuracion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.Habilitado).HasColumnName("habilitado").IsRequired();
        builder.Property(x => x.Produccion).HasColumnName("produccion").IsRequired();
        builder.Property(x => x.UsaCaeaPorDefecto).HasColumnName("usa_caea_por_defecto").IsRequired();
        builder.Property(x => x.CuitEmisor).HasColumnName("cuit_emisor").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CertificadoAlias).HasColumnName("certificado_alias").HasMaxLength(200);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.SucursalId, x.PuntoFacturacionId }).IsUnique();
    }
}
