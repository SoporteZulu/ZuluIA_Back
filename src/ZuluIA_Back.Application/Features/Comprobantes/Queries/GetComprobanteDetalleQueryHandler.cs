using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobanteDetalleQueryHandler(
    IComprobanteRepository repo,
    IImputacionRepository imputRepo,
    IApplicationDbContext db)
    : IRequestHandler<GetComprobanteDetalleQuery, ComprobanteDetalleDto?>
{
    public async Task<ComprobanteDetalleDto?> Handle(
        GetComprobanteDetalleQuery request,
        CancellationToken ct)
    {
        var comp = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (comp is null) return null;

        // Lookups paralelos
        var terceroTask = db.Terceros.AsNoTrackingSafe()
            .Where(x => x.Id == comp.TerceroId)
            .Select(x => new { x.RazonSocial, x.NroDocumento, x.Legajo })
            .FirstOrDefaultSafeAsync(ct);

        var tipoTask = db.TiposComprobante.AsNoTrackingSafe()
            .Where(x => x.Id == comp.TipoComprobanteId)
            .Select(x => new { x.Descripcion, x.Codigo })
            .FirstOrDefaultSafeAsync(ct);

        var monedaTask = db.Monedas.AsNoTrackingSafe()
            .Where(x => x.Id == comp.MonedaId)
            .Select(x => new { x.Simbolo })
            .FirstOrDefaultSafeAsync(ct);

        var sucursalTask = db.Sucursales.AsNoTrackingSafe()
            .Where(x => x.Id == comp.SucursalId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultSafeAsync(ct);

        await Task.WhenAll(terceroTask, tipoTask, monedaTask, sucursalTask);

        var tercero = await terceroTask;
        var tipo = await tipoTask;
        var moneda = await monedaTask;
        var sucursal = await sucursalTask;

        // Autorizador de devolución (si existe)
        string? autorizadorNombre = null;
        if (comp.AutorizadorDevolucionId.HasValue)
        {
            autorizadorNombre = await db.Usuarios.AsNoTrackingSafe()
                .Where(x => x.Id == comp.AutorizadorDevolucionId.Value)
                .Select(x => x.NombreCompleto)
                .FirstOrDefaultSafeAsync(ct);
        }

        // Condición IVA del tercero
        var condIva = await db.CondicionesIva
            .AsNoTrackingSafe()
            .Join(db.Terceros.AsNoTrackingSafe().Where(t => t.Id == comp.TerceroId),
                c => c.Id,
                t => t.CondicionIvaId,
                (c, _) => c.Descripcion)
            .FirstOrDefaultSafeAsync(ct);

        var cot = await db.ComprobantesCot
            .AsNoTrackingSafe()
            .Where(x => x.ComprobanteId == comp.Id)
            .Select(x => new ComprobanteCotDto
            {
                Numero = x.Numero,
                FechaVigencia = x.FechaVigencia,
                Descripcion = x.Descripcion
            })
            .FirstOrDefaultSafeAsync(ct);

        var atributosRemito = await db.ComprobantesAtributos
            .AsNoTrackingSafe()
            .Where(x => x.ComprobanteId == comp.Id)
            .OrderBy(x => x.Clave)
            .Select(x => new ComprobanteAtributoDto
            {
                Id = x.Id,
                Clave = x.Clave,
                Valor = x.Valor,
                TipoDato = x.TipoDato
            })
            .ToListSafeAsync(ct);

        var depositoOrigenDescripcion = comp.DepositoOrigenId.HasValue
            ? await db.Depositos.AsNoTrackingSafe()
                .Where(x => x.Id == comp.DepositoOrigenId.Value)
                .Select(x => x.Descripcion)
                .FirstOrDefaultSafeAsync(ct)
            : null;

        // Ítems con descripciones
        var itemIds = comp.Items.Select(x => x.ItemId).Distinct().ToList();
        var depositoIds = comp.Items.Where(x => x.DepositoId.HasValue)
                              .Select(x => x.DepositoId!.Value).Distinct().ToList();
        var unidadIds = comp.Items.Where(x => x.UnidadMedidaId.HasValue)
            .Select(x => x.UnidadMedidaId!.Value)
            .Distinct()
            .ToList();

        var terceroRelacionadoIds = new[] { comp.VendedorId, comp.CobradorId, comp.TransporteId }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var usuarioIds = new[] { comp.CreatedBy, comp.UpdatedBy, comp.UsuarioAnulacionId }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var items = await db.Items.AsNoTrackingSafe()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var depositos = await db.Depositos.AsNoTrackingSafe()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var unidades = await db.UnidadesMedida.AsNoTrackingSafe()
            .Where(x => unidadIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var tercerosRelacionados = await db.Terceros.AsNoTrackingSafe()
            .Where(x => terceroRelacionadoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.Legajo })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var usuarios = await db.Usuarios.AsNoTrackingSafe()
            .Where(x => usuarioIds.Contains(x.Id))
            .Select(x => new { x.Id, x.UserName, x.NombreCompleto })
            .ToDictionarySafeAsync(x => x.Id, x => x.NombreCompleto ?? x.UserName, ct);

        var comprobanteItemIds = comp.Items.Select(x => x.Id).ToList();
        var atributosPorItem = await db.ComprobantesItemsAtributos.AsNoTrackingSafe()
            .Where(x => comprobanteItemIds.Contains(x.ComprobanteItemId))
            .Join(db.AtributosComerciales.AsNoTrackingSafe(),
                a => a.AtributoComercialId,
                d => d.Id,
                (a, d) => new
                {
                    a.Id,
                    a.ComprobanteItemId,
                    a.AtributoComercialId,
                    d.Codigo,
                    d.Descripcion,
                    a.Valor
                })
            .ToListSafeAsync(ct);

        var atributosLookup = atributosPorItem
            .GroupBy(x => x.ComprobanteItemId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<ComprobanteItemAtributoDto>)g.Select(x => new ComprobanteItemAtributoDto
                {
                    Id = x.Id,
                    AtributoComercialId = x.AtributoComercialId,
                    AtributoCodigo = x.Codigo,
                    AtributoDescripcion = x.Descripcion,
                    Valor = x.Valor
                }).ToList().AsReadOnly());

        // Imputaciones
        var imputaciones = await imputRepo.GetByComprobanteDestinoAsync(comp.Id, true, ct);
        var origenIds = imputaciones.Select(x => x.ComprobanteOrigenId).ToList();

        var numerosOrigen = await db.Comprobantes.AsNoTrackingSafe()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionarySafeAsync(x => x.Id, ct);

        var motivoDebito = comp.MotivoDebitoId.HasValue
            ? await db.MotivosDebito.AsNoTrackingSafe()
                .Where(x => x.Id == comp.MotivoDebitoId.Value)
                .Select(x => new { x.Descripcion, x.EsFiscal })
                .FirstOrDefaultSafeAsync(ct)
            : null;

        var comprobanteOrigen = comp.ComprobanteOrigenId.HasValue
            ? await db.Comprobantes.AsNoTrackingSafe()
                .Where(x => x.Id == comp.ComprobanteOrigenId.Value)
                .Select(x => new { x.Id, x.Fecha, x.TipoComprobanteId, x.Numero.Prefijo, x.Numero.Numero })
                .FirstOrDefaultSafeAsync(ct)
            : null;

        var tipoComprobanteOrigen = comprobanteOrigen is not null
            ? await db.TiposComprobante.AsNoTrackingSafe()
                .Where(x => x.Id == comprobanteOrigen.TipoComprobanteId)
                .Select(x => x.Descripcion)
                .FirstOrDefaultSafeAsync(ct)
            : null;

        string? vendedorNombre = null;
        string? vendedorLegajo = null;
        if (comp.VendedorId.HasValue && tercerosRelacionados.TryGetValue(comp.VendedorId.Value, out var vendedor))
        {
            vendedorNombre = vendedor.RazonSocial;
            vendedorLegajo = vendedor.Legajo;
        }

        string? cobradorNombre = null;
        string? cobradorLegajo = null;
        if (comp.CobradorId.HasValue && tercerosRelacionados.TryGetValue(comp.CobradorId.Value, out var cobrador))
        {
            cobradorNombre = cobrador.RazonSocial;
            cobradorLegajo = cobrador.Legajo;
        }

        string? transporteRazonSocial = null;
        if (comp.TransporteId.HasValue && tercerosRelacionados.TryGetValue(comp.TransporteId.Value, out var transporte))
        {
            transporteRazonSocial = transporte.RazonSocial;
        }

        var imputDtos = imputaciones.Select(i => new ImputacionDto
        {
            Id                   = i.Id,
            ComprobanteOrigenId  = i.ComprobanteOrigenId,
            NumeroOrigen         = numerosOrigen.ContainsKey(i.ComprobanteOrigenId)
                ? $"{numerosOrigen[i.ComprobanteOrigenId].Prefijo:D4}-{numerosOrigen[i.ComprobanteOrigenId].Numero:D8}"
                : "—",
            ComprobanteDestinoId = i.ComprobanteDestinoId,
            NumeroDestino        = comp.Numero.Formateado,
            Importe              = i.Importe,
            Fecha                = i.Fecha,
            CreatedAt            = i.CreatedAt
        }).ToList();

        var tieneIdentificadoresSifen = comp.SifenTrackingId != null
            || comp.SifenCdc != null
            || comp.SifenNumeroLote != null;

        return new ComprobanteDetalleDto
        {
            Id                         = comp.Id,
            SucursalId                 = comp.SucursalId,
            SucursalRazonSocial        = sucursal?.RazonSocial ?? "—",
            PuntoFacturacionId         = comp.PuntoFacturacionId,
            TipoComprobanteId          = comp.TipoComprobanteId,
            TipoComprobanteDescripcion = tipo?.Descripcion ?? "—",
            TipoComprobanteCodigo      = tipo?.Codigo,
            Prefijo                    = comp.Numero.Prefijo,
            Numero                     = comp.Numero.Numero,
            NumeroFormateado           = comp.Numero.Formateado,
            Fecha                      = comp.Fecha,
            FechaVencimiento           = comp.FechaVencimiento,
            TerceroId                  = comp.TerceroId,
            TerceroRazonSocial         = tercero?.RazonSocial ?? "—",
            TerceroCuit                = tercero?.NroDocumento ?? "—",
            TerceroLegajo              = tercero?.Legajo,
            TerceroCondicionIva        = condIva ?? "—",
            TerceroDomicilioSnapshot   = comp.TerceroDomicilioSnapshot,
            MonedaId                   = comp.MonedaId,
            MonedaSimbolo              = moneda?.Simbolo ?? "$",
            Cotizacion                 = comp.Cotizacion,
            
            // Campos comerciales
            VendedorId                 = comp.VendedorId,
            VendedorNombre             = vendedorNombre,
            VendedorLegajo             = vendedorLegajo,
            CobradorId                 = comp.CobradorId,
            CobradorNombre             = cobradorNombre,
            CobradorLegajo             = cobradorLegajo,
            ZonaComercialId            = comp.ZonaComercialId,
            ListaPreciosId             = comp.ListaPreciosId,
            CondicionPagoId            = comp.CondicionPagoId,
            PlazoDias                  = comp.PlazoDias,
            CanalVentaId               = comp.CanalVentaId,
            PorcentajeComisionVendedor = comp.PorcentajeComisionVendedor,
            PorcentajeComisionCobrador = comp.PorcentajeComisionCobrador,
            MotivoDebitoId             = comp.MotivoDebitoId,
            MotivoDebitoDescripcion    = motivoDebito?.Descripcion,
            MotivoDebitoObservacion    = comp.MotivoDebitoObservacion,
            MotivoDebitoEsFiscal       = motivoDebito?.EsFiscal,
            
            // Campos logísticos
            TransporteId               = comp.TransporteId,
            TransporteRazonSocial      = transporteRazonSocial,
            ChoferNombre               = comp.ChoferNombre,
            ChoferDni                  = comp.ChoferDni,
            PatVehiculo                = comp.PatVehiculo,
            PatAcoplado                = comp.PatAcoplado,
            RutaLogistica              = comp.RutaLogistica,
            DomicilioEntrega           = comp.DomicilioEntrega,
            ObservacionesLogisticas    = comp.ObservacionesLogisticas,
            FechaEstimadaEntrega       = comp.FechaEstimadaEntrega,
            FechaRealEntrega           = comp.FechaRealEntrega,
            FirmaConformidad           = comp.FirmaConformidad,
            NombreQuienRecibe          = comp.NombreQuienRecibe,
            DniQuienRecibe             = comp.DniQuienRecibe,
            EstadoLogistico            = comp.EstadoLogistico,
            EsValorizado               = comp.EsValorizado,
            DepositoOrigenId           = comp.DepositoOrigenId,
            DepositoOrigenDescripcion  = depositoOrigenDescripcion,
            CotNumero                  = cot?.Numero,
            CotFechaVigencia           = cot?.FechaVigencia,
            CotDescripcion             = cot?.Descripcion,
            Cot                        = cot,
            PesoTotal                  = comp.PesoTotal,
            VolumenTotal               = comp.VolumenTotal,
            Bultos                     = comp.Bultos,
            TipoEmbalaje               = comp.TipoEmbalaje,
            SeguroTransporte           = comp.SeguroTransporte,
            ValorDeclarado             = comp.ValorDeclarado,
            
            // Observaciones extendidas
            ObservacionInterna         = comp.ObservacionInterna,
            ObservacionFiscal          = comp.ObservacionFiscal,
            
            // Recargo y descuento global
            RecargoPorcentaje          = comp.RecargoPorcentaje,
            RecargoImporte             = comp.RecargoImporte,
            DescuentoPorcentaje        = comp.DescuentoPorcentaje,
            
            Subtotal                   = comp.Subtotal,
            DescuentoImporte           = comp.DescuentoImporte,
            NetoGravado                = comp.NetoGravado,
            NetoNoGravado              = comp.NetoNoGravado,
            IvaRi                      = comp.IvaRi,
            IvaRni                     = comp.IvaRni,
            Percepciones               = comp.Percepciones,
            Retenciones                = comp.Retenciones,
            Total                      = comp.Total,
            Saldo                      = comp.Saldo,
            TimbradoId                 = comp.TimbradoId,
            NroTimbrado                = comp.NroTimbrado,
            EstadoSifen                = comp.EstadoSifen,
            SifenCodigoRespuesta       = comp.SifenCodigoRespuesta,
            SifenMensajeRespuesta      = comp.SifenMensajeRespuesta,
            SifenTrackingId            = comp.SifenTrackingId,
            SifenCdc                   = comp.SifenCdc,
            SifenNumeroLote            = comp.SifenNumeroLote,
            SifenFechaRespuesta        = comp.SifenFechaRespuesta,
            TieneIdentificadoresSifen  = tieneIdentificadoresSifen,
            PuedeReintentarSifen       = comp.EstadoSifen == EstadoSifenParaguay.Rechazado
                || comp.EstadoSifen == EstadoSifenParaguay.Error,
            PuedeConciliarSifen        = comp.Estado != EstadoComprobante.Borrador
                && comp.EstadoSifen != EstadoSifenParaguay.Aceptado
                && tieneIdentificadoresSifen,
            Cae                        = comp.Cae,
            Caea                       = comp.Caea,
            FechaVtoCae                = comp.FechaVtoCae,
            QrData                     = comp.QrData,
            EstadoAfip                 = comp.EstadoAfip.ToString().ToUpperInvariant(),
            UltimoErrorAfip            = comp.UltimoErrorAfip,
            FechaUltimaConsultaAfip    = comp.FechaUltimaConsultaAfip,
            Estado                     = comp.Estado.ToString().ToUpperInvariant(),
            Observacion                = comp.Observacion,
            CreatedAt                  = comp.CreatedAt,
            UpdatedAt                  = comp.UpdatedAt,
            CreatedBy                  = comp.CreatedBy,
            CreatedByUsuario           = comp.CreatedBy.HasValue ? usuarios.GetValueOrDefault(comp.CreatedBy.Value) : null,
            UpdatedBy                  = comp.UpdatedBy,
            UpdatedByUsuario           = comp.UpdatedBy.HasValue ? usuarios.GetValueOrDefault(comp.UpdatedBy.Value) : null,
            FechaAnulacion             = comp.FechaAnulacion,
            UsuarioAnulacionId         = comp.UsuarioAnulacionId,
            UsuarioAnulacionNombre     = comp.UsuarioAnulacionId.HasValue ? usuarios.GetValueOrDefault(comp.UsuarioAnulacionId.Value) : null,
            MotivoAnulacion            = comp.MotivoAnulacion,
            ComprobanteOrigenId        = comp.ComprobanteOrigenId,
            ComprobanteOrigenNumero    = comprobanteOrigen is null ? null : $"{comprobanteOrigen.Prefijo:D4}-{comprobanteOrigen.Numero:D8}",
            ComprobanteOrigenTipo      = tipoComprobanteOrigen,
            ComprobanteOrigenFecha     = comprobanteOrigen?.Fecha,
            
            Items = comp.Items.OrderBy(x => x.Orden).Select(i => new ComprobanteItemDto
            {
                Id                 = i.Id,
                ItemId             = i.ItemId,
                ItemCodigo         = items.GetValueOrDefault(i.ItemId)?.Codigo ?? "—",
                Descripcion        = i.Descripcion,
                Cantidad           = i.Cantidad,
                CantidadBonificada = i.CantidadBonificada,
                PrecioUnitario     = i.PrecioUnitario,
                DescuentoPct       = i.DescuentoPct,
                AlicuotaIvaId      = i.AlicuotaIvaId,
                PorcentajeIva      = i.PorcentajeIva,
                SubtotalNeto       = i.SubtotalNeto,
                IvaImporte         = i.IvaImporte,
                TotalLinea         = i.TotalLinea,
                DepositoId         = i.DepositoId,
                DepositoDescripcion = i.DepositoId.HasValue
                    ? depositos.GetValueOrDefault(i.DepositoId.Value)?.Descripcion
                    : null,
                Orden              = i.Orden,
                EsGravado          = i.EsGravado,
                
                // Campos extendidos
                Lote                    = i.Lote,
                Serie                   = i.Serie,
                FechaVencimiento        = i.FechaVencimiento,
                UnidadMedidaId          = i.UnidadMedidaId,
                UnidadMedidaDescripcion = i.UnidadMedidaId.HasValue
                    ? unidades.GetValueOrDefault(i.UnidadMedidaId.Value)?.Descripcion
                    : null,
                ObservacionRenglon      = i.ObservacionRenglon,
                PrecioListaOriginal     = i.PrecioListaOriginal,
                ComisionVendedorRenglon = i.ComisionVendedorRenglon,
                ComprobanteItemOrigenId = i.ComprobanteItemOrigenId,
                CantidadDocumentoOrigen = i.CantidadDocumentoOrigen,
                PrecioDocumentoOrigen   = i.PrecioDocumentoOrigen,
                
                // Campos de cumplimiento
                CantidadEntregada       = i.CantidadEntregada,
                CantidadPendiente       = i.CantidadPendiente,
                EstadoEntrega           = i.EstadoEntrega,
                EstadoEntregaDescripcion = i.EstadoEntrega?.ToString(),
                EsAtrasado              = i.EsAtrasado,
                Diferencia              = i.ObtenerDiferencia(),
                
                Atributos          = atributosLookup.GetValueOrDefault(i.Id) ?? []
            }).ToList().AsReadOnly(),
            
            // Metadatos de devolución
            MotivoDevolucion              = comp.MotivoDevolucion,
            MotivoDevolucionDescripcion   = comp.MotivoDevolucion?.ToString(),
            TipoDevolucion                = comp.TipoDevolucion,
            TipoDevolucionDescripcion     = comp.TipoDevolucion?.ToString(),
            AutorizadorDevolucionId       = comp.AutorizadorDevolucionId,
            AutorizadorDevolucionNombre   = autorizadorNombre,
            FechaAutorizacionDevolucion   = comp.FechaAutorizacionDevolucion,
            ObservacionDevolucion         = comp.ObservacionDevolucion,
            ReingresaStock                = comp.ReingresaStock,
            AcreditaCuentaCorriente       = comp.AcreditaCuentaCorriente,
            
            // Metadatos de pedido
            EstadoPedido                  = comp.EstadoPedido,
            EstadoPedidoDescripcion       = comp.EstadoPedido?.ToString(),
            FechaEntregaCompromiso        = comp.FechaEntregaCompromiso,
            TienePedidosAtrasados         = comp.Items.Any(i => i.EsAtrasado),
            PorcentajeCumplimiento        = comp.Items.Any() 
                ? Math.Round((comp.Items.Sum(i => i.CantidadEntregada) / comp.Items.Sum(i => i.Cantidad)) * 100, 2)
                : 0,
            AtributosRemito               = atributosRemito.AsReadOnly(),
            
            Imputaciones = imputDtos.AsReadOnly()
        };
    }
}