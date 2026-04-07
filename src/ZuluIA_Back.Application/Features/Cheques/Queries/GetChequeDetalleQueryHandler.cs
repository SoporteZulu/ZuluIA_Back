using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.DTOs;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequeDetalleQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetChequeDetalleQuery, ChequeDetalleDto?>
{
    public async Task<ChequeDetalleDto?> Handle(
        GetChequeDetalleQuery request,
        CancellationToken ct)
    {
        var cheque = await db.Cheques.AsNoTracking()
            .Where(x => x.Id == request.ChequeId)
            .Select(c => new ChequeDetalleDto
            {
                Id = c.Id,
                CajaId = c.CajaId,
                CajaDescripcion = db.CajasCuentasBancarias
                    .Where(caj => caj.Id == c.CajaId)
                    .Select(caj => caj.Descripcion)
                    .FirstOrDefault() ?? "—",
                TerceroId = c.TerceroId,
                TerceroRazonSocial = c.TerceroId.HasValue
                    ? db.Terceros.Where(t => t.Id == c.TerceroId).Select(t => t.RazonSocial).FirstOrDefault()
                    : null,
                TerceroNroDocumento = c.TerceroId.HasValue
                    ? db.Terceros.Where(t => t.Id == c.TerceroId).Select(t => t.NroDocumento).FirstOrDefault()
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
                MonedaSimbolo = db.Monedas.Where(m => m.Id == c.MonedaId).Select(m => m.Simbolo).FirstOrDefault() ?? "$",
                MonedaDescripcion = db.Monedas.Where(m => m.Id == c.MonedaId).Select(m => m.Descripcion).FirstOrDefault() ?? "—",
                Estado = c.Estado.ToString().ToUpperInvariant(),
                Tipo = c.Tipo.ToString().ToUpperInvariant(),
                EsALaOrden = c.EsALaOrden,
                EsCruzado = c.EsCruzado,
                ChequeraId = c.ChequeraId,
                ChequeraDescripcion = c.ChequeraId.HasValue
                    ? db.Chequeras.Where(ch => ch.Id == c.ChequeraId).Select(ch => ch.Banco).FirstOrDefault()
                    : null,
                ChequeraBancoDescripcion = c.ChequeraId.HasValue
                    ? db.Chequeras.Where(ch => ch.Id == c.ChequeraId).Select(ch => ch.Banco).FirstOrDefault()
                    : null,
                ChequeraNroCuenta = c.ChequeraId.HasValue
                    ? db.Chequeras.Where(ch => ch.Id == c.ChequeraId).Select(ch => ch.NroCuenta).FirstOrDefault()
                    : null,
                ComprobanteOrigenId = c.ComprobanteOrigenId,
                ComprobanteOrigenNumero = c.ComprobanteOrigenId.HasValue
                    ? db.Comprobantes.Where(comp => comp.Id == c.ComprobanteOrigenId).Select(comp => comp.Numero.Formateado).FirstOrDefault()
                    : null,
                ComprobanteOrigenTipo = c.ComprobanteOrigenId.HasValue
                    ? db.Comprobantes
                        .Where(comp => comp.Id == c.ComprobanteOrigenId)
                        .Join(db.TiposComprobante,
                            comp => comp.TipoComprobanteId,
                            tc => tc.Id,
                            (_, tc) => tc.Descripcion)
                        .FirstOrDefault()
                    : null,
                ComprobanteOrigenFecha = c.ComprobanteOrigenId.HasValue
                    ? db.Comprobantes.Where(comp => comp.Id == c.ComprobanteOrigenId).Select(comp => comp.Fecha).FirstOrDefault()
                    : null,
                Observacion = c.Observacion,
                ConceptoRechazo = c.ConceptoRechazo,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                CreatedByUsuario = c.CreatedBy.HasValue
                    ? db.Usuarios.Where(u => u.Id == c.CreatedBy).Select(u => u.NombreCompleto ?? u.UserName).FirstOrDefault()
                    : null,
                UpdatedBy = c.UpdatedBy,
                UpdatedByUsuario = c.UpdatedBy.HasValue
                    ? db.Usuarios.Where(u => u.Id == c.UpdatedBy).Select(u => u.NombreCompleto ?? u.UserName).FirstOrDefault()
                    : null
            })
            .FirstOrDefaultAsync(ct);

        if (cheque is null)
            return null;

        var historial = await db.ChequesHistorial.AsNoTracking()
            .Where(h => h.ChequeId == request.ChequeId)
            .OrderByDescending(h => h.FechaOperacion)
            .Select(h => new ChequeHistorialDto
            {
                Id = h.Id,
                ChequeId = h.ChequeId,
                CajaId = h.CajaId,
                CajaDescripcion = db.CajasCuentasBancarias
                    .Where(caj => caj.Id == h.CajaId)
                    .Select(caj => caj.Descripcion)
                    .FirstOrDefault() ?? "—",
                TerceroId = h.TerceroId,
                TerceroRazonSocial = h.TerceroId.HasValue
                    ? db.Terceros.Where(t => t.Id == h.TerceroId).Select(t => t.RazonSocial).FirstOrDefault()
                    : null,
                Operacion = h.Operacion.ToString().ToUpperInvariant(),
                EstadoAnterior = h.EstadoAnterior.HasValue ? h.EstadoAnterior.Value.ToString().ToUpperInvariant() : null,
                EstadoNuevo = h.EstadoNuevo.ToString().ToUpperInvariant(),
                FechaOperacion = h.FechaOperacion,
                FechaAcreditacion = h.FechaAcreditacion,
                Observacion = h.Observacion,
                CreatedAt = h.CreatedAt,
                CreatedBy = h.CreatedBy
            })
            .ToListAsync(ct);

        cheque.Historial = historial.AsReadOnly();
        return cheque;
    }
}
