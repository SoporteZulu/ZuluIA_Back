using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetComprobanteSifenEstadoQuery(long Id) : IRequest<ComprobanteSifenEstadoDto?>;

public class GetComprobanteSifenEstadoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComprobanteSifenEstadoQuery, ComprobanteSifenEstadoDto?>
{
    public async Task<ComprobanteSifenEstadoDto?> Handle(GetComprobanteSifenEstadoQuery request, CancellationToken ct)
    {
        return await db.Comprobantes
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ComprobanteSifenEstadoDto
            {
                ComprobanteId = x.Id,
                EstadoComprobante = x.Estado,
                EstadoSifen = x.EstadoSifen,
                SifenCodigoRespuesta = x.SifenCodigoRespuesta,
                SifenMensajeRespuesta = x.SifenMensajeRespuesta,
                SifenTrackingId = x.SifenTrackingId,
                SifenCdc = x.SifenCdc,
                SifenNumeroLote = x.SifenNumeroLote,
                SifenFechaRespuesta = x.SifenFechaRespuesta,
                TimbradoId = x.TimbradoId,
                NroTimbrado = x.NroTimbrado,
                FueAceptado = x.EstadoSifen == EstadoSifenParaguay.Aceptado,
                TieneIdentificadores = x.SifenTrackingId != null
                    || x.SifenCdc != null
                    || x.SifenNumeroLote != null,
                PuedeReintentar = x.EstadoSifen == EstadoSifenParaguay.Rechazado
                    || x.EstadoSifen == EstadoSifenParaguay.Error,
                PuedeConciliar = x.Estado != EstadoComprobante.Borrador
                    && x.EstadoSifen != EstadoSifenParaguay.Aceptado
                    && (x.SifenTrackingId != null || x.SifenCdc != null || x.SifenNumeroLote != null)
            })
            .FirstOrDefaultAsync(ct);
    }
}