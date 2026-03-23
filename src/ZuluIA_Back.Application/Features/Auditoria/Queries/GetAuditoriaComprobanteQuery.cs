using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Auditoria.Queries;

public record AuditoriaComprobanteDto(
    long Id,
    long ComprobanteId,
    long? UsuarioId,
    string Accion,
    DateTime FechaHora,
    string? DetalleCambio,
    string? IpOrigen);

public record GetAuditoriaComprobanteQuery(long ComprobanteId) : IRequest<List<AuditoriaComprobanteDto>>;

public class GetAuditoriaComprobanteQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAuditoriaComprobanteQuery, List<AuditoriaComprobanteDto>>
{
    public async Task<List<AuditoriaComprobanteDto>> Handle(
        GetAuditoriaComprobanteQuery request, CancellationToken ct)
    {
        return await db.AuditoriaComprobantes.AsNoTracking()
            .Where(a => a.ComprobanteId == request.ComprobanteId)
            .OrderByDescending(a => a.FechaHora)
            .Select(a => new AuditoriaComprobanteDto(
                a.Id, a.ComprobanteId, a.UsuarioId,
                a.Accion.ToString(), a.FechaHora, a.DetalleCambio, a.IpOrigen))
            .ToListAsync(ct);
    }
}
