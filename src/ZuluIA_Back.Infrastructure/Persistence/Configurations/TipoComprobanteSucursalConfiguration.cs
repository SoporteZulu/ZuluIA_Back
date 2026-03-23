using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoComprobanteSucursalConfiguration : IEntityTypeConfiguration<TipoComprobanteSucursal>
{
    public void Configure(EntityTypeBuilder<TipoComprobanteSucursal> builder)
    {
        builder.ToTable("tipo_comprobante_sucursal");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TipoComprobanteId)
               .HasColumnName("tipo_comprobante_id").IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id");

        builder.Property(x => x.NumeroComprobanteProximo)
               .HasColumnName("numero_comprobante_proximo").IsRequired();

        builder.Property(x => x.FilasCantidad)
               .HasColumnName("filas_cantidad").IsRequired();

        builder.Property(x => x.FilasAnchoMaximo)
               .HasColumnName("filas_ancho_maximo").IsRequired();

        builder.Property(x => x.CantidadCopias)
               .HasColumnName("cantidad_copias").IsRequired();

        builder.Property(x => x.ImprimirControladorFiscal)
               .HasColumnName("imprimir_controlador_fiscal").IsRequired();

        builder.Property(x => x.VarianteNroUnico)
               .HasColumnName("variante_nro_unico").IsRequired();

        builder.Property(x => x.PermitirSeleccionMoneda)
               .HasColumnName("permitir_seleccion_moneda").IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id");

        builder.Property(x => x.Editable)
               .HasColumnName("editable").IsRequired();

        builder.Property(x => x.VistaPrevia)
               .HasColumnName("vista_previa").IsRequired();

        builder.Property(x => x.ControlIntervalo)
               .HasColumnName("control_intervalo").IsRequired();

        builder.Property(x => x.NumeroDesde)
               .HasColumnName("numero_desde");

        builder.Property(x => x.NumeroHasta)
               .HasColumnName("numero_hasta");

        builder.HasIndex(x => new { x.TipoComprobanteId, x.SucursalId }).IsUnique();
    }
}
