using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoComprobantePuntoFacturacionConfiguration
    : IEntityTypeConfiguration<TipoComprobantePuntoFacturacion>
{
    public void Configure(EntityTypeBuilder<TipoComprobantePuntoFacturacion> builder)
    {
        builder.ToTable("TIPOSCOMPROBANTESPUNTOFACTURACION");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.PuntoFacturacionId)
               .IsRequired()
               .HasColumnName("pfac_id");

        builder.Property(e => e.TipoComprobanteId)
               .IsRequired()
               .HasColumnName("id_tipocomprobante");

        builder.Property(e => e.NumeroComprobanteProximo)
               .HasColumnName("NumeroComprobanteProximo");

        builder.Property(e => e.Editable)
               .HasColumnName("editable");

        builder.Property(e => e.FilasCantidad)
               .HasColumnName("FilasCantidad");

        builder.Property(e => e.FilasAnchoMaximo)
               .HasColumnName("FilasAnchoMaximo");

        builder.Property(e => e.ReporteId)
               .HasColumnName("id_reporte");

        builder.Property(e => e.CantidadCopias)
               .HasColumnName("CantidadCopias");

        builder.Property(e => e.VistaPrevia)
               .HasColumnName("VistaPrevia");

        builder.Property(e => e.ImprimirControladorFiscal)
               .HasColumnName("ImprimirControladorFiscal");

        builder.Property(e => e.PermitirSeleccionMoneda)
               .HasColumnName("PermitirSeleccionMoneda");

        builder.Property(e => e.VarianteNroUnico)
               .HasColumnName("VarianteNroUnico");

        builder.Property(e => e.MascaraMoneda)
               .HasMaxLength(50)
               .HasColumnName("Mascara_Moneda");

        builder.HasIndex(e => new { e.PuntoFacturacionId, e.TipoComprobanteId })
               .IsUnique();
    }
}
