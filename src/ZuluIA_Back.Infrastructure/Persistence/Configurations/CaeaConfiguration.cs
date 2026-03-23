using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CaeaConfiguration : IEntityTypeConfiguration<Caea>
{
    public void Configure(EntityTypeBuilder<Caea> builder)
    {
        builder.ToTable("caeas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PuntoFacturacionId).HasColumnName("punto_facturacion_id").IsRequired();
        builder.Property(x => x.NroCaea).HasColumnName("nro_caea").HasMaxLength(15).IsRequired();
        builder.Property(x => x.FechaDesde).HasColumnName("fecha_desde").IsRequired();
        builder.Property(x => x.FechaHasta).HasColumnName("fecha_hasta").IsRequired();
        builder.Property(x => x.FechaProcesoAfip).HasColumnName("fecha_proceso_afip");
        builder.Property(x => x.FechaTopeInformarAfip).HasColumnName("fecha_tope_informar_afip");
        builder.Property(x => x.TipoComprobante).HasColumnName("tipo_comprobante").HasMaxLength(5).IsRequired();
        builder.Property(x => x.CantidadAsignada).HasColumnName("cantidad_asignada").IsRequired();
        builder.Property(x => x.CantidadUsada).HasColumnName("cantidad_usada");

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoCaea>(v, true))
               .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.PuntoFacturacionId);
        builder.HasIndex(new[] { "nro_caea", "punto_facturacion_id" }).IsUnique();
    }
}
