using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ConfiguracionFiscalConfiguration : IEntityTypeConfiguration<ConfiguracionFiscal>
{
    public void Configure(EntityTypeBuilder<ConfiguracionFiscal> builder)
    {
        builder.ToTable("FISC_CONFIGURACIONES_GENERALES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("fcg_id").UseIdentityColumn();

        builder.Property(x => x.PuntoFacturacionId).HasColumnName("fcg_pfac_id").IsRequired();
        builder.Property(x => x.TipoComprobanteId).HasColumnName("fcg_id_tipocomprobante").IsRequired();
        builder.Property(x => x.TecnologiaId).HasColumnName("tec_id");
        builder.Property(x => x.InterfazFiscalId).HasColumnName("if_id");
        builder.Property(x => x.Marco).HasColumnName("fcg_marco");
        builder.Property(x => x.Puerto).HasColumnName("fcg_puerto").HasMaxLength(50);
        builder.Property(x => x.CopiasDefault).HasColumnName("fcg_copias_default").HasDefaultValue(2);
        builder.Property(x => x.ClaveActivacion).HasColumnName("fcg_clave_activacion").HasMaxLength(200);
        builder.Property(x => x.DirectorioLocal).HasColumnName("fcg_directorio_local").HasMaxLength(500);
        builder.Property(x => x.DirectorioLocalBackup).HasColumnName("fcg_directorio_local_backup").HasMaxLength(500);
        builder.Property(x => x.TimerInicial).HasColumnName("fcg_timer_inicial");
        builder.Property(x => x.TimerSincronizacion).HasColumnName("fcg_timer_sincronizacion");
        builder.Property(x => x.Online).HasColumnName("fcg_online").HasDefaultValue(false);

        builder.HasIndex(x => new { x.PuntoFacturacionId, x.TipoComprobanteId });
    }
}
