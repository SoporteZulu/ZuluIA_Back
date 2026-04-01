using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public class GetMotivosDebitoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMotivosDebitoQuery, IReadOnlyList<MotivoDebitoDto>>
{
    public async Task<IReadOnlyList<MotivoDebitoDto>> Handle(GetMotivosDebitoQuery request, CancellationToken ct)
    {
        var query = db.MotivosDebito.AsNoTracking();

        if (request.SoloActivos)
            query = query.Where(x => x.Activo);

        return await query
            .OrderBy(x => x.Descripcion)
            .Select(x => new MotivoDebitoDto
            {
                Id = x.Id,
                Codigo = x.Codigo,
                Descripcion = x.Descripcion,
                EsFiscal = x.EsFiscal,
                RequiereDocumentoOrigen = x.RequiereDocumentoOrigen,
                AfectaCuentaCorriente = x.AfectaCuentaCorriente,
                Activo = x.Activo
            })
            .ToListAsync(ct);
    }
}
