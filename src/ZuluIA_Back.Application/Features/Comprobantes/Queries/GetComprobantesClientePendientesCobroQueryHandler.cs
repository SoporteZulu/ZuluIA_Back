using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobantesClientePendientesCobroQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComprobantesClientePendientesCobroQuery, IReadOnlyList<ComprobantePendienteCobroDto>>
{
    public async Task<IReadOnlyList<ComprobantePendienteCobroDto>> Handle(
        GetComprobantesClientePendientesCobroQuery request,
        CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var fechaHasta = request.FechaHasta ?? hoy;

        var query = db.Comprobantes.AsNoTracking()
            .Where(c => c.TerceroId == request.TerceroId)
            .Where(c => c.Saldo > 0)
            .Where(c => c.Estado == EstadoComprobante.Emitido || c.Estado == EstadoComprobante.PagadoParcial)
            .Where(c => c.Fecha <= fechaHasta);

        if (request.SucursalId.HasValue)
            query = query.Where(c => c.SucursalId == request.SucursalId.Value);

        if (request.MonedaId.HasValue)
            query = query.Where(c => c.MonedaId == request.MonedaId.Value);

        if (request.SoloVencidos)
            query = query.Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento < hoy);

        var comprobantes = await query
            .Join(db.TiposComprobante.AsNoTracking(),
                c => c.TipoComprobanteId,
                t => t.Id,
                (c, t) => new { Comprobante = c, Tipo = t })
            .Join(db.Monedas.AsNoTracking(),
                x => x.Comprobante.MonedaId,
                m => m.Id,
                (x, m) => new
                {
                    x.Comprobante,
                    TipoDescripcion = x.Tipo.Descripcion,
                    TipoCodigo = x.Tipo.Codigo,
                    MonedaSimbolo = m.Simbolo
                })
            .OrderBy(x => x.Comprobante.FechaVencimiento ?? x.Comprobante.Fecha)
            .ThenBy(x => x.Comprobante.Fecha)
            .ToListAsync(ct);

        return comprobantes
            .Select(x => new ComprobantePendienteCobroDto
            {
                Id = x.Comprobante.Id,
                TipoComprobante = x.TipoDescripcion,
                TipoComprobanteCodigo = x.TipoCodigo,
                Prefijo = x.Comprobante.Numero.Prefijo,
                Numero = x.Comprobante.Numero.Numero,
                NumeroFormateado = x.Comprobante.Numero.Formateado,
                FechaEmision = x.Comprobante.Fecha,
                FechaVencimiento = x.Comprobante.FechaVencimiento,
                DiasMora = x.Comprobante.FechaVencimiento.HasValue
                    ? hoy.DayNumber - x.Comprobante.FechaVencimiento.Value.DayNumber
                    : null,
                EstaVencido = x.Comprobante.FechaVencimiento.HasValue
                    && x.Comprobante.FechaVencimiento < hoy,
                MonedaId = x.Comprobante.MonedaId,
                MonedaSimbolo = x.MonedaSimbolo,
                Cotizacion = x.Comprobante.Cotizacion,
                ImporteTotal = x.Comprobante.Total,
                ImporteCobrado = x.Comprobante.Total - x.Comprobante.Saldo,
                SaldoPendiente = x.Comprobante.Saldo,
                Observacion = x.Comprobante.Observacion,
                ImporteAImputar = 0
            })
            .ToList()
            .AsReadOnly();
    }
}
