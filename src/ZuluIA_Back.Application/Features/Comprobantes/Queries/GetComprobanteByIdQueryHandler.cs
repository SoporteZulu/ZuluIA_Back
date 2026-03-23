using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobanteByIdQueryHandler(
    IComprobanteRepository repo,
    IApplicationDbContext db,
    IMapper mapper)
    : IRequestHandler<GetComprobanteByIdQuery, ComprobanteDto?>
{
    public async Task<ComprobanteDto?> Handle(GetComprobanteByIdQuery request, CancellationToken ct)
    {
        var comp = await repo.GetByIdAsync(request.Id, ct);
        if (comp is null)
            return null;

        var dto = mapper.Map<ComprobanteDto>(comp);
        dto.Impuestos = await db.ComprobantesImpuestos
            .AsNoTracking()
            .Where(x => x.ComprobanteId == comp.Id)
            .OrderBy(x => x.AlicuotaIvaId)
            .Select(x => new ComprobanteImpuestoDto
            {
                AlicuotaIvaId = x.AlicuotaIvaId,
                PorcentajeIva = x.PorcentajeIva,
                BaseImponible = x.BaseImponible,
                ImporteIva = x.ImporteIva
            })
            .ToListAsync(ct);

        dto.Tributos = await db.ComprobantesTributos
            .AsNoTracking()
            .Where(x => x.ComprobanteId == comp.Id)
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Id)
            .Select(x => new ComprobanteTributoDto
            {
                ImpuestoId = x.ImpuestoId,
                Codigo = x.Codigo,
                Descripcion = x.Descripcion,
                BaseImponible = x.BaseImponible,
                Alicuota = x.Alicuota,
                Importe = x.Importe,
                Orden = x.Orden
            })
            .ToListAsync(ct);

        return dto;
    }
}