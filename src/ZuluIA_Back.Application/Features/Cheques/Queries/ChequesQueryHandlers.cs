using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequesPendientesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetChequesPendientesQuery, IReadOnlyList<ChequePendienteDto>>
{
    public async Task<IReadOnlyList<ChequePendienteDto>> Handle(
        GetChequesPendientesQuery request,
        CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var query = db.Cheques.AsNoTracking()
            .Where(c => c.Estado == EstadoCheque.Cartera && c.Tipo == TipoCheque.Tercero);

        if (request.CajaId.HasValue)
            query = query.Where(c => c.CajaId == request.CajaId.Value);

        if (request.HastaFechaVencimiento.HasValue)
            query = query.Where(c => c.FechaVencimiento <= request.HastaFechaVencimiento.Value);

        if (request.SoloVencidos)
            query = query.Where(c => c.FechaVencimiento < hoy);

        var cheques = await query
            .OrderBy(c => c.FechaVencimiento)
            .Select(c => new ChequePendienteDto
            {
                Id = c.Id,
                NroCheque = c.NroCheque,
                Banco = c.Banco,
                Titular = c.Titular,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                Importe = c.Importe,
                MonedaSimbolo = db.Monedas.Where(m => m.Id == c.MonedaId).Select(m => m.Simbolo).FirstOrDefault() ?? "$",
                CajaId = c.CajaId,
                CajaDescripcion = db.CajasCuentasBancarias
                    .Where(caj => caj.Id == c.CajaId)
                    .Select(caj => caj.Descripcion)
                    .FirstOrDefault() ?? "—",
                TerceroId = c.TerceroId,
                TerceroRazonSocial = c.TerceroId.HasValue
                    ? db.Terceros.Where(t => t.Id == c.TerceroId).Select(t => t.RazonSocial).FirstOrDefault()
                    : null,
                EsCruzado = c.EsCruzado,
                EsALaOrden = c.EsALaOrden,
                DiasHastaVencimiento = (c.FechaVencimiento.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days,
                EstaVencido = c.FechaVencimiento < hoy
            })
            .ToListAsync(ct);

        return cheques.AsReadOnly();
    }
}

public class GetChequesDepositadosQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetChequesDepositadosQuery, IReadOnlyList<ChequeDto>>
{
    public async Task<IReadOnlyList<ChequeDto>> Handle(
        GetChequesDepositadosQuery request,
        CancellationToken ct)
    {
        var query = db.Cheques.AsNoTracking()
            .Where(c => c.Estado == EstadoCheque.Depositado || c.Estado == EstadoCheque.EnTransito);

        if (request.CajaId.HasValue)
            query = query.Where(c => c.CajaId == request.CajaId.Value);

        if (request.Desde.HasValue)
            query = query.Where(c => c.FechaDeposito >= request.Desde.Value);

        if (request.Hasta.HasValue)
            query = query.Where(c => c.FechaDeposito <= request.Hasta.Value);

        var cheques = await query
            .OrderBy(c => c.FechaDeposito)
            .Select(c => new ChequeDto
            {
                Id = c.Id,
                CajaId = c.CajaId,
                CajaDescripcion = db.CajasCuentasBancarias
                    .Where(caj => caj.Id == c.CajaId)
                    .Select(caj => caj.Descripcion)
                    .FirstOrDefault(),
                TerceroId = c.TerceroId,
                TerceroRazonSocial = c.TerceroId.HasValue
                    ? db.Terceros.Where(t => t.Id == c.TerceroId).Select(t => t.RazonSocial).FirstOrDefault()
                    : null,
                NroCheque = c.NroCheque,
                Banco = c.Banco,
                CodigoSucursalBancaria = c.CodigoSucursalBancaria,
                CodigoPostal = c.CodigoPostal,
                Titular = c.Titular,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                FechaAcreditacion = c.FechaAcreditacion,
                FechaDeposito = c.FechaDeposito,
                Importe = c.Importe,
                MonedaId = c.MonedaId,
                MonedaSimbolo = db.Monedas.Where(m => m.Id == c.MonedaId).Select(m => m.Simbolo).FirstOrDefault(),
                Estado = c.Estado.ToString().ToUpperInvariant(),
                Tipo = c.Tipo.ToString().ToUpperInvariant(),
                EsALaOrden = c.EsALaOrden,
                EsCruzado = c.EsCruzado,
                ChequeraId = c.ChequeraId,
                ComprobanteOrigenId = c.ComprobanteOrigenId,
                Observacion = c.Observacion,
                ConceptoRechazo = c.ConceptoRechazo,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(ct);

        return cheques.AsReadOnly();
    }
}

public class GetChequeHistorialQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetChequeHistorialQuery, IReadOnlyList<ChequeRutaItemDto>>
{
    public async Task<IReadOnlyList<ChequeRutaItemDto>> Handle(
        GetChequeHistorialQuery request,
        CancellationToken ct)
    {
        var historial = await db.ChequesHistorial.AsNoTracking()
            .Where(h => h.ChequeId == request.ChequeId)
            .OrderByDescending(h => h.FechaOperacion)
            .Select(h => new ChequeRutaItemDto
            {
                Id = h.Id,
                Fecha = h.CreatedAt,
                Operacion = h.Operacion.ToString().ToUpperInvariant(),
                EstadoAnterior = h.EstadoAnterior.HasValue ? h.EstadoAnterior.Value.ToString().ToUpperInvariant() : "—",
                EstadoNuevo = h.EstadoNuevo.ToString().ToUpperInvariant(),
                UsuarioId = h.CreatedBy,
                UsuarioNombre = h.CreatedBy.HasValue
                    ? db.Usuarios.Where(u => u.Id == h.CreatedBy).Select(u => u.NombreCompleto ?? u.UserName).FirstOrDefault()
                    : null,
                Observacion = h.Observacion
            })
            .ToListAsync(ct);

        return historial.AsReadOnly();
    }
}
