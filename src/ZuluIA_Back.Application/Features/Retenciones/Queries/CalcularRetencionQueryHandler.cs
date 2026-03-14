using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Retenciones.Queries;

public class CalcularRetencionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<CalcularRetencionQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(
        CalcularRetencionQuery request,
        CancellationToken ct)
    {
        var tipo = await db.TiposRetencion
            .Include(x => x.Escalas)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TipoRetencionId && x.Activo, ct);

        if (tipo is null)
            return Result.Failure<decimal>(
                $"No se encontró el tipo de retención con ID {request.TipoRetencionId}.");

        var importe = tipo.CalcularImporte(request.BaseImponible);
        return Result.Success(importe);
    }
}
