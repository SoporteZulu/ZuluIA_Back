using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetComprobanteSifenHistorialQuery(long ComprobanteId) : IRequest<List<ComprobanteSifenHistorialDto>>;

public class GetComprobanteSifenHistorialQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetComprobanteSifenHistorialQuery, List<ComprobanteSifenHistorialDto>>
{
    public async Task<List<ComprobanteSifenHistorialDto>> Handle(GetComprobanteSifenHistorialQuery request, CancellationToken ct)
    {
        return await db.HistorialSifenComprobantes
            .AsNoTracking()
            .Where(x => x.ComprobanteId == request.ComprobanteId)
            .OrderByDescending(x => x.FechaHora)
            .Select(x => new ComprobanteSifenHistorialDto
            {
                Id = x.Id,
                ComprobanteId = x.ComprobanteId,
                UsuarioId = x.UsuarioId,
                EstadoSifen = x.EstadoSifen,
                Aceptado = x.Aceptado,
                EstadoRespuesta = x.EstadoRespuesta,
                CodigoRespuesta = x.CodigoRespuesta,
                MensajeRespuesta = x.MensajeRespuesta,
                TrackingId = x.TrackingId,
                Cdc = x.Cdc,
                NumeroLote = x.NumeroLote,
                FechaHora = x.FechaHora,
                FechaRespuesta = x.FechaRespuesta,
                Detalle = x.Detalle,
                RespuestaCruda = x.RespuestaCruda
            })
            .ToListAsync(ct);
    }
}