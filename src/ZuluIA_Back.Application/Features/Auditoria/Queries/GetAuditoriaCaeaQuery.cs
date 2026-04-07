using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Auditoria.Queries;

public record AuditoriaCaeaDto(
    long Id,
    long CaeaId,
    long? UsuarioId,
    string Accion,
    DateTime FechaHora,
    string? DetalleCambio,
    string? IpOrigen);

public record GetAuditoriaCaeaQuery(long CaeaId) : IRequest<List<AuditoriaCaeaDto>>;

public class GetAuditoriaCaeaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAuditoriaCaeaQuery, List<AuditoriaCaeaDto>>
{
    public async Task<List<AuditoriaCaeaDto>> Handle(GetAuditoriaCaeaQuery request, CancellationToken ct)
    {
        return await db.AuditoriaCaeas.AsNoTracking()
            .Where(a => a.CaeaId == request.CaeaId)
            .OrderByDescending(a => a.FechaHora)
            .Select(a => new AuditoriaCaeaDto(
                a.Id,
                a.CaeaId,
                a.UsuarioId,
                a.Accion.ToString(),
                a.FechaHora,
                a.DetalleCambio,
                a.IpOrigen))
            .ToListAsync(ct);
    }
}