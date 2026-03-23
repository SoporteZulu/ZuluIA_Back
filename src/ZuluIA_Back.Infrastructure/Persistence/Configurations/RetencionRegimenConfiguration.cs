using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RetencionRegimenConfiguration : IEntityTypeConfiguration<RetencionRegimen>
{
    public void Configure(EntityTypeBuilder<RetencionRegimen> builder)
    {
        builder.ToTable("retencion_regimen");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo").HasMaxLength(50).IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion").HasMaxLength(500);

        builder.Property(x => x.RetencionId)
               .HasColumnName("retencion_id").IsRequired();

        builder.Property(x => x.ControlTipoComprobante)
               .HasColumnName("control_tipo_comprobante");
        builder.Property(x => x.ControlTipoComprobanteAplica)
               .HasColumnName("control_tipo_comprobante_aplica");
        builder.Property(x => x.BaseImponibleComposicion)
               .HasColumnName("base_imponible_composicion").HasMaxLength(200);
        builder.Property(x => x.NoImponible)
               .HasColumnName("no_imponible").HasColumnType("numeric(18,4)");
        builder.Property(x => x.NoImponibleAplica)
               .HasColumnName("no_imponible_aplica");
        builder.Property(x => x.BaseImponiblePorcentaje)
               .HasColumnName("base_imponible_porcentaje").HasColumnType("numeric(10,4)");
        builder.Property(x => x.BaseImponiblePorcentajeAplica)
               .HasColumnName("base_imponible_porcentaje_aplica");
        builder.Property(x => x.BaseImponibleMinimo)
               .HasColumnName("base_imponible_minimo").HasColumnType("numeric(18,4)");
        builder.Property(x => x.BaseImponibleMinimoAplica)
               .HasColumnName("base_imponible_minimo_aplica");
        builder.Property(x => x.BaseImponibleMaximo)
               .HasColumnName("base_imponible_maximo").HasColumnType("numeric(18,4)");
        builder.Property(x => x.BaseImponibleMaximoAplica)
               .HasColumnName("base_imponible_maximo_aplica");
        builder.Property(x => x.RetencionComposicion)
               .HasColumnName("retencion_composicion").HasMaxLength(200);
        builder.Property(x => x.RetencionMinimo)
               .HasColumnName("retencion_minimo").HasColumnType("numeric(18,4)");
        builder.Property(x => x.RetencionMinimoAplica)
               .HasColumnName("retencion_minimo_aplica");
        builder.Property(x => x.RetencionMaximo)
               .HasColumnName("retencion_maximo").HasColumnType("numeric(18,4)");
        builder.Property(x => x.RetencionMaximoAplica)
               .HasColumnName("retencion_maximo_aplica");
        builder.Property(x => x.Alicuota)
               .HasColumnName("alicuota").HasColumnType("numeric(10,4)");
        builder.Property(x => x.AlicuotaAplica)
               .HasColumnName("alicuota_aplica");
        builder.Property(x => x.AlicuotaEscalaAplica)
               .HasColumnName("alicuota_escala_aplica");
        builder.Property(x => x.AlicuotaConvenio)
               .HasColumnName("alicuota_convenio").HasColumnType("numeric(10,4)");
        builder.Property(x => x.AlicuotaConvenioAplica)
               .HasColumnName("alicuota_convenio_aplica");

        builder.HasIndex(x => new { x.RetencionId, x.Codigo }).IsUnique();
    }
}
