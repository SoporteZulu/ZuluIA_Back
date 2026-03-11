using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public class GetLibroIvaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetLibroIvaQuery, LibroIvaDto>
{
    public async Task<LibroIvaDto> Handle(
        GetLibroIvaQuery request,
        CancellationToken ct)
    {
        // Filtrar tipos de comprobante según tipo de libro
        var tiposQuery = db.TiposComprobante.AsNoTracking();
        tiposQuery = request.Tipo == TipoLibroIva.Ventas
            ? tiposQuery.Where(t => t.EsVenta && !t.EsInterno)
            : tiposQuery.Where(t => t.EsCompra && !t.EsInterno);

        var tipoIds = await tiposQuery
            .Select(t => t.Id)
            .ToListAsync(ct);

        // Obtener comprobantes del período en estado Emitido o PagadoParcial o Pagado
        var estadosValidos = new[] { "EMITIDO", "PAGADOPARCIAL", "PAGADO" };

        var comprobantes = await db.Comprobantes
            .AsNoTracking()
            .Where(c =>
                c.SucursalId == request.SucursalId         &&
                c.Fecha      >= request.Desde              &&
                c.Fecha      <= request.Hasta              &&
                tipoIds.Contains(c.TipoComprobanteId)      &&
                estadosValidos.Contains(c.Estado.ToString().ToUpperInvariant()))
            .Select(c => new
            {
                c.Fecha,
                c.TipoComprobanteId,
                c.Numero.Prefijo,
                Numero = c.Numero.Numero,
                c.TerceroId,
                c.NetoGravado,
                c.NetoNoGravado,
                c.IvaRi,
                c.IvaRni,
                c.Percepciones,
                c.Total
            })
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Prefijo)
            .ThenBy(c => c.Numero)
            .ToListAsync(ct);

        // Obtener datos de terceros y tipos
        var terceroIds = comprobantes.Select(c => c.TerceroId).Distinct().ToList();
        var tipoIdsUsados = comprobantes.Select(c => c.TipoComprobanteId).Distinct().ToList();

        var terceros = await db.Terceros
            .AsNoTracking()
            .Where(t => terceroIds.Contains(t.Id))
            .Select(t => new { t.Id, t.RazonSocial, t.NroDocumento })
            .ToDictionaryAsync(t => t.Id, ct);

        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(t => tipoIdsUsados.Contains(t.Id))
            .Select(t => new { t.Id, t.Descripcion })
            .ToDictionaryAsync(t => t.Id, ct);

        var condicionesIva = await db.CondicionesIva
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Descripcion, ct);

        var lineas = comprobantes.Select(c =>
        {
            var tercero = terceros.GetValueOrDefault(c.TerceroId);
            var tipo = tipos.GetValueOrDefault(c.TipoComprobanteId);

            return new LineaLibroIvaDto
            {
                Fecha            = c.Fecha,
                TipoComprobante  = tipo?.Descripcion ?? "—",
                Prefijo          = c.Prefijo,
                Numero           = c.Numero,
                NumeroFormateado = $"{c.Prefijo:D4}-{c.Numero:D8}",
                RazonSocial      = tercero?.RazonSocial ?? "—",
                Cuit             = tercero?.NroDocumento ?? "—",
                CondicionIva     = "—",
                Neto             = c.NetoGravado + c.NetoNoGravado,
                Iva              = c.IvaRi + c.IvaRni,
                Percepciones     = c.Percepciones,
                Total            = c.Total
            };
        }).ToList();

        return new LibroIvaDto
        {
            Desde                = request.Desde,
            Hasta                = request.Hasta,
            SucursalId           = request.SucursalId,
            TipoLibro            = request.Tipo.ToString(),
            CantidadComprobantes = lineas.Count,
            TotalNeto            = lineas.Sum(l => l.Neto),
            TotalIva             = lineas.Sum(l => l.Iva),
            TotalPercepciones    = lineas.Sum(l => l.Percepciones),
            TotalGeneral         = lineas.Sum(l => l.Total),
            Lineas               = lineas.AsReadOnly()
        };
    }
}