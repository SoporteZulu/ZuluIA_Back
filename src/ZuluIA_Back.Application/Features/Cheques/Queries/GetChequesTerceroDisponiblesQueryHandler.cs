using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequesTerceroDisponiblesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetChequesTerceroDisponiblesQuery, IReadOnlyList<ChequeDisponibleDto>>
{
    public async Task<IReadOnlyList<ChequeDisponibleDto>> Handle(
        GetChequesTerceroDisponiblesQuery request,
        CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.Cheques.AsNoTracking()
            .Where(ch => ch.Tipo == TipoCheque.Tercero)
            .Where(ch => ch.Estado == EstadoCheque.Cartera)
            .Where(ch => ch.FechaVencimiento <= hoy.AddDays(180));

        if (request.MonedaId.HasValue)
            query = query.Where(ch => ch.MonedaId == request.MonedaId.Value);

        if (request.ImporteMinimo.HasValue)
            query = query.Where(ch => ch.Importe >= request.ImporteMinimo.Value);

        if (request.ImporteMaximo.HasValue)
            query = query.Where(ch => ch.Importe <= request.ImporteMaximo.Value);

        if (request.FechaPagoDesde.HasValue)
            query = query.Where(ch => ch.FechaVencimiento >= request.FechaPagoDesde.Value);

        if (request.FechaPagoHasta.HasValue)
            query = query.Where(ch => ch.FechaVencimiento <= request.FechaPagoHasta.Value);

        if (!string.IsNullOrWhiteSpace(request.Plaza))
            query = query.Where(ch => ch.Plaza == request.Plaza);

        var cheques = await query
            .Join(db.Monedas.AsNoTracking(),
                ch => ch.MonedaId,
                m => m.Id,
                (ch, m) => new { Cheque = ch, Moneda = m })
            .Join(db.Terceros.AsNoTracking(),
                x => x.Cheque.TerceroId,
                t => t.Id,
                (x, t) => new
                {
                    x.Cheque,
                    MonedaSimbolo = x.Moneda.Simbolo,
                    ClienteOrigenId = t.Id,
                    ClienteOrigenRazonSocial = t.RazonSocial
                })
            .OrderBy(x => x.Cheque.FechaVencimiento)
            .ThenBy(x => x.Cheque.Importe)
            .ToListAsync(ct);

        return cheques
            .Select(x => new ChequeDisponibleDto
            {
                Id = x.Cheque.Id,
                NumeroCheque = x.Cheque.NroCheque,
                BancoId = 0,
                BancoDescripcion = x.Cheque.Banco,
                SucursalBancaria = x.Cheque.CodigoSucursalBancaria,
                Plaza = x.Cheque.Plaza ?? "—",
                Cuit = x.Cheque.Cuit ?? "—",
                NombreTitular = x.Cheque.Titular ?? "—",
                FechaEmision = x.Cheque.FechaEmision,
                FechaPago = x.Cheque.FechaVencimiento,
                DiasAlCobro = x.Cheque.FechaVencimiento.DayNumber - hoy.DayNumber,
                EsDiferido = x.Cheque.FechaVencimiento > hoy.AddDays(1),
                MonedaId = x.Cheque.MonedaId,
                MonedaSimbolo = x.MonedaSimbolo,
                Importe = x.Cheque.Importe,
                EstadoCheque = x.Cheque.Estado.ToString(),
                ClienteOrigenId = x.ClienteOrigenId,
                ClienteOrigenRazonSocial = x.ClienteOrigenRazonSocial,
                CobroOrigenId = x.Cheque.CobroOrigenId,
                FechaRecepcion = x.Cheque.FechaEmision,
                Observaciones = x.Cheque.Observacion
            })
            .ToList()
            .AsReadOnly();
    }
}
