using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public class GetCobroDetalleQueryHandler(
    ICobroRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetCobroDetalleQuery, CobroDto?>
{
    public async Task<CobroDto?> Handle(
        GetCobroDetalleQuery request,
        CancellationToken ct)
    {
        var cobro = await repo.GetByIdConMediosAsync(request.Id, ct);
        if (cobro is null) return null;

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == cobro.TerceroId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultAsync(ct);

        var moneda = await db.Monedas.AsNoTracking()
            .Where(x => x.Id == cobro.MonedaId)
            .Select(x => new { x.Simbolo })
            .FirstOrDefaultAsync(ct);

        var cajaIds = cobro.Medios.Select(m => m.CajaId).Distinct().ToList();
        var formaPagoIds = cobro.Medios.Select(m => m.FormaPagoId).Distinct().ToList();
        var monedaIds = cobro.Medios.Select(m => m.MonedaId).Distinct().ToList();

        var cajas = await db.CajasCuentasBancarias.AsNoTracking()
            .Where(x => cajaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var formasPago = await db.FormasPago.AsNoTracking()
            .Where(x => formaPagoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        return new CobroDto
        {
            Id                 = cobro.Id,
            SucursalId         = cobro.SucursalId,
            TerceroId          = cobro.TerceroId,
            TerceroRazonSocial = tercero?.RazonSocial ?? "—",
            Fecha              = cobro.Fecha,
            MonedaId           = cobro.MonedaId,
            MonedaSimbolo      = moneda?.Simbolo ?? "$",
            Cotizacion         = cobro.Cotizacion,
            Total              = cobro.Total,
            Observacion        = cobro.Observacion,
            Estado             = cobro.Estado.ToString().ToUpperInvariant(),
            NroCierre          = cobro.NroCierre,
            CreatedAt          = cobro.CreatedAt,
            Medios = cobro.Medios.Select(m => new CobroMedioDto
            {
                Id                   = m.Id,
                CajaId               = m.CajaId,
                CajaDescripcion      = cajas.GetValueOrDefault(m.CajaId)?.Descripcion ?? "—",
                FormaPagoId          = m.FormaPagoId,
                FormaPagoDescripcion = formasPago.GetValueOrDefault(m.FormaPagoId)?.Descripcion ?? "—",
                ChequeId             = m.ChequeId,
                Importe              = m.Importe,
                MonedaId             = m.MonedaId,
                MonedaSimbolo        = monedas.GetValueOrDefault(m.MonedaId)?.Simbolo ?? "$",
                Cotizacion           = m.Cotizacion
            }).ToList().AsReadOnly()
        };
    }
}