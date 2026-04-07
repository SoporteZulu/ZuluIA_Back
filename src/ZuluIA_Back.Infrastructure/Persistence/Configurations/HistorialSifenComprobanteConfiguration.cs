using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class HistorialSifenComprobanteConfiguration : IEntityTypeConfiguration<HistorialSifenComprobante>
{
    public void Configure(EntityTypeBuilder<HistorialSifenComprobante> builder)
    {
        builder.ToTable("historial_sifen_comprobantes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.EstadoSifen)
            .HasColumnName("estado_sifen")
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<EstadoSifenParaguay>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Aceptado).HasColumnName("aceptado").IsRequired();
        builder.Property(x => x.EstadoRespuesta).HasColumnName("estado_respuesta").HasMaxLength(100);
        builder.Property(x => x.CodigoRespuesta).HasColumnName("codigo_respuesta").HasMaxLength(100);
        builder.Property(x => x.MensajeRespuesta).HasColumnName("mensaje_respuesta").HasMaxLength(500);
        builder.Property(x => x.TrackingId).HasColumnName("tracking_id").HasMaxLength(100);
        builder.Property(x => x.Cdc).HasColumnName("cdc").HasMaxLength(100);
        builder.Property(x => x.NumeroLote).HasColumnName("numero_lote").HasMaxLength(100);
        builder.Property(x => x.FechaHora).HasColumnName("fecha_hora").IsRequired();
        builder.Property(x => x.FechaRespuesta).HasColumnName("fecha_respuesta");
        builder.Property(x => x.Detalle).HasColumnName("detalle").HasMaxLength(1000);
        builder.Property(x => x.RespuestaCruda).HasColumnName("respuesta_cruda").HasMaxLength(4000);

        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => new { x.ComprobanteId, x.FechaHora });
        builder.HasIndex(x => x.TrackingId);
        builder.HasIndex(x => x.Cdc);
    }
}