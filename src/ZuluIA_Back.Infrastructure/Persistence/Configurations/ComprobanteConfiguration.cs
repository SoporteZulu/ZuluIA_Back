using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("comprobantes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id").IsRequired();

        builder.Property(x => x.PuntoFacturacionId)
               .HasColumnName("punto_facturacion_id");

        builder.Property(x => x.TipoComprobanteId)
               .HasColumnName("tipo_comprobante_id").IsRequired();

        // Value Object NumeroComprobante → columnas prefijo + numero
        builder.OwnsOne(x => x.Numero, n =>
        {
            n.Property(x => x.Prefijo)
             .HasColumnName("prefijo")
             .IsRequired();

            n.Property(x => x.Numero)
             .HasColumnName("numero")
             .IsRequired();
        });

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.FechaVencimiento)
               .HasColumnName("fecha_vencimiento");

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id").IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion")
               .HasPrecision(18, 6)
               .HasDefaultValue(1m);

        // Datos comerciales
        builder.Property(x => x.VendedorId)
               .HasColumnName("vendedor_id");

        builder.Property(x => x.CobradorId)
               .HasColumnName("cobrador_id");

        builder.Property(x => x.ZonaComercialId)
               .HasColumnName("zona_comercial_id");

        builder.Property(x => x.ListaPreciosId)
               .HasColumnName("lista_precios_id");

        builder.Property(x => x.CondicionPagoId)
               .HasColumnName("condicion_pago_id");

        builder.Property(x => x.PlazoDias)
               .HasColumnName("plazo_dias");

        builder.Property(x => x.CanalVentaId)
               .HasColumnName("canal_venta_id");

        builder.Property(x => x.PorcentajeComisionVendedor)
               .HasColumnName("porcentaje_comision_vendedor")
               .HasPrecision(5, 2);

        builder.Property(x => x.PorcentajeComisionCobrador)
               .HasColumnName("porcentaje_comision_cobrador")
               .HasPrecision(5, 2);

        // Datos logísticos
        builder.Property(x => x.TransporteId)
               .HasColumnName("transporte_id");

        builder.Property(x => x.ChoferNombre)
               .HasColumnName("chofer_nombre")
               .HasMaxLength(200);

        builder.Property(x => x.ChoferDni)
               .HasColumnName("chofer_dni")
               .HasMaxLength(20);

        builder.Property(x => x.PatVehiculo)
               .HasColumnName("pat_vehiculo")
               .HasMaxLength(20);

        builder.Property(x => x.PatAcoplado)
               .HasColumnName("pat_acoplado")
               .HasMaxLength(20);

        builder.Property(x => x.DomicilioEntrega)
               .HasColumnName("domicilio_entrega")
               .HasMaxLength(500);

        builder.Property(x => x.ObservacionesLogisticas)
               .HasColumnName("observaciones_logisticas")
               .HasMaxLength(1000);

        builder.Property(x => x.FechaEstimadaEntrega)
               .HasColumnName("fecha_estimada_entrega");

        builder.Property(x => x.FechaRealEntrega)
               .HasColumnName("fecha_real_entrega");

        builder.Property(x => x.FirmaConformidad)
               .HasColumnName("firma_conformidad")
               .HasMaxLength(500);

        builder.Property(x => x.NombreQuienRecibe)
               .HasColumnName("nombre_quien_recibe")
               .HasMaxLength(200);

        builder.Property(x => x.EstadoLogistico)
               .HasColumnName("estado_logistico")
               .HasConversion(
                   v => v.HasValue ? v.Value.ToString().ToUpperInvariant() : null,
                   v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<EstadoLogisticoRemito>(v, true))
               .HasMaxLength(30);

        builder.Property(x => x.EsValorizado)
               .HasColumnName("es_valorizado")
               .HasDefaultValue(true);

        builder.Property(x => x.DepositoOrigenId)
               .HasColumnName("deposito_origen_id");

        // Datos de pedido
        builder.Property(x => x.FechaEntregaCompromiso)
               .HasColumnName("fecha_entrega_compromiso");

        builder.Property(x => x.EstadoPedido)
               .HasColumnName("estado_pedido")
               .HasConversion(
                   v => v.HasValue ? (int)v.Value : (int?)null,
                   v => v.HasValue ? (EstadoPedido)v.Value : null);

        builder.Property(x => x.MotivoCierrePedido)
               .HasColumnName("motivo_cierre_pedido")
               .HasMaxLength(500);

        builder.Property(x => x.FechaCierrePedido)
               .HasColumnName("fecha_cierre_pedido");

        // Datos adicionales comerciales/fiscales
        builder.Property(x => x.ObservacionInterna)
               .HasColumnName("observacion_interna")
               .HasMaxLength(1000);

        builder.Property(x => x.ObservacionFiscal)
               .HasColumnName("observacion_fiscal")
               .HasMaxLength(1000);

        builder.Property(x => x.RecargoPorcentaje)
               .HasColumnName("recargo_porcentaje")
               .HasPrecision(5, 2);

        builder.Property(x => x.RecargoImporte)
               .HasColumnName("recargo_importe")
               .HasPrecision(18, 4);

        builder.Property(x => x.DescuentoPorcentaje)
               .HasColumnName("descuento_porcentaje")
               .HasPrecision(5, 2);

        builder.Property(x => x.TerceroDomicilioSnapshot)
               .HasColumnName("tercero_domicilio_snapshot")
               .HasMaxLength(500);

        // Datos específicos de Notas de Débito
        builder.Property(x => x.MotivoDebitoId)
               .HasColumnName("motivo_debito_id");

        builder.Property(x => x.MotivoDebitoObservacion)
               .HasColumnName("motivo_debito_observacion")
               .HasMaxLength(1000);

        // Auditoría de anulación
        builder.Property(x => x.FechaAnulacion)
               .HasColumnName("fecha_anulacion");

        builder.Property(x => x.UsuarioAnulacionId)
               .HasColumnName("usuario_anulacion_id");

        builder.Property(x => x.MotivoAnulacion)
               .HasColumnName("motivo_anulacion")
               .HasMaxLength(500);

        // Navegación a ítems
        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey(x => x.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Cot)
               .WithOne(x => x.Comprobante)
               .HasForeignKey<ComprobanteCot>(x => x.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Atributos)
               .WithOne(x => x.Comprobante)
               .HasForeignKey(x => x.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Atributos)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new
        {
            x.SucursalId,
            x.TipoComprobanteId
        });
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.EstadoLogistico);
        builder.HasIndex(x => x.DepositoOrigenId);
        builder.HasIndex(x => x.EstadoAfip);
        builder.HasIndex(x => x.Cae);
        builder.HasIndex(x => x.EstadoSifen);
        builder.HasIndex(x => x.EstadoPedido)
               .HasFilter("estado_pedido IS NOT NULL");
        builder.HasIndex(x => x.FechaEntregaCompromiso)
               .HasFilter("fecha_entrega_compromiso IS NOT NULL");
        builder.HasIndex(x => new { x.EstadoPedido, x.FechaEntregaCompromiso })
               .HasFilter("estado_pedido IN (0, 1)");
        builder.HasIndex(x => x.SifenTrackingId);
        builder.HasIndex(x => x.SifenCdc);
        builder.HasIndex(x => x.SifenCodigoRespuesta);
        builder.HasIndex(x => x.MotivoDevolucion)
               .HasFilter("motivo_devolucion IS NOT NULL");
        builder.HasIndex(x => x.TipoDevolucion)
               .HasFilter("tipo_devolucion IS NOT NULL");
        builder.HasIndex(x => x.AutorizadorDevolucionId)
               .HasFilter("autorizador_devolucion_id IS NOT NULL");
        builder.HasIndex(x => new { x.ComprobanteOrigenId, x.MotivoDevolucion })
               .HasFilter("motivo_devolucion IS NOT NULL");
        builder.HasIndex(x => x.MotivoDebitoId)
               .HasFilter("motivo_debito_id IS NOT NULL");
        builder.HasIndex(x => x.FechaAnulacion)
               .HasFilter("fecha_anulacion IS NOT NULL");
        builder.HasIndex(x => new { x.TipoComprobanteId, x.Estado, x.MotivoDebitoId })
               .HasFilter("motivo_debito_id IS NOT NULL");
    }
}
