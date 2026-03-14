using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;

public class GetOrdenPreparacionByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdenPreparacionByIdQuery, Result<OrdenPreparacionDto>>
{
    public async Task<Result<OrdenPreparacionDto>> Handle(
        GetOrdenPreparacionByIdQuery request,
        CancellationToken ct)
    {
        var orden = await db.OrdenesPreparacion
            .AsNoTracking()
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);

        if (orden is null)
            return Result.Failure<OrdenPreparacionDto>($"No se encontró la orden de preparación con ID {request.Id}.");

        var dto = new OrdenPreparacionDto
        {
            Id                  = orden.Id,
            SucursalId          = orden.SucursalId,
            ComprobanteOrigenId = orden.ComprobanteOrigenId,
            TerceroId           = orden.TerceroId,
            Fecha               = orden.Fecha,
            Estado              = orden.Estado,
            Observacion         = orden.Observacion,
            FechaConfirmacion   = orden.FechaConfirmacion,
            Detalles            = orden.Detalles.Select(d => new OrdenPreparacionDetalleDto(
                d.Id,
                d.ItemId,
                d.DepositoId,
                d.Cantidad,
                d.CantidadEntregada,
                d.EstaCompleto,
                d.Observacion
            )).ToList()
        };

        return Result.Success(dto);
    }
}
