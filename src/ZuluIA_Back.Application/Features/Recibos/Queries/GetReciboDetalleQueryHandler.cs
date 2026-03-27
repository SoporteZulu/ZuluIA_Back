using MediatR;
using ZuluIA_Back.Application.Features.Recibos.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Recibos.Queries;

public class GetReciboDetalleQueryHandler(IReciboRepository reciboRepo)
    : IRequestHandler<GetReciboDetalleQuery, ReciboDto?>
{
    public async Task<ReciboDto?> Handle(GetReciboDetalleQuery request, CancellationToken ct)
    {
        var recibo = await reciboRepo.GetByIdConItemsAsync(request.Id, ct);
        if (recibo is null) return null;

        return new ReciboDto
        {
            Id                 = recibo.Id,
            SucursalId         = recibo.SucursalId,
            TerceroId          = recibo.TerceroId,
            TerceroRazonSocial = string.Empty,
            Fecha              = recibo.Fecha,
            Serie              = recibo.Serie,
            Numero             = recibo.Numero,
            Total              = recibo.Total,
            Estado             = recibo.Estado.ToString(),
            CobroId            = recibo.CobroId,
            Observacion        = recibo.Observacion,
            CreatedAt          = recibo.CreatedAt,
            Items              = recibo.Items.Select(i => new ReciboItemDto
            {
                Id          = i.Id,
                Descripcion = i.Descripcion,
                Importe     = i.Importe
            }).ToList()
        };
    }
}
