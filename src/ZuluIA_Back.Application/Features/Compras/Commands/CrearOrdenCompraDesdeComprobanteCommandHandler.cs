using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearOrdenCompraDesdeComprobanteCommandHandler(
    IApplicationDbContext db,
    IComprobanteRepository comprobanteRepo,
    IRepository<OrdenCompraMeta> ordenCompraRepo,
    IUnitOfWork uow)
    : IRequestHandler<CrearOrdenCompraDesdeComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearOrdenCompraDesdeComprobanteCommand request, CancellationToken ct)
    {
        var ordenExistente = await db.OrdenesCompraMeta
            .AsNoTracking()
            .AnyAsync(x => x.ComprobanteId == request.ComprobanteId, ct);

        if (ordenExistente)
            return Result.Failure<long>("El comprobante seleccionado ya está vinculado a una orden de compra.");

        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure<long>($"No se encontró el comprobante ID {request.ComprobanteId}.");

        if (comprobante.Estado == EstadoComprobante.Anulado)
            return Result.Failure<long>("No se puede crear una orden de compra desde un comprobante anulado.");

        if (comprobante.TerceroId != request.ProveedorId)
            return Result.Failure<long>("El proveedor seleccionado no coincide con el comprobante base.");

        var cantidadTotal = comprobante.Items.Sum(x => x.Cantidad - x.CantidadBonificada);

        try
        {
            var orden = OrdenCompraMeta.Crear(
                comprobante.Id,
                request.ProveedorId,
                request.FechaEntregaReq,
                request.CondicionesEntrega,
                cantidadTotal);

            await ordenCompraRepo.AddAsync(orden, ct);
            await uow.SaveChangesAsync(ct);

            return Result.Success(orden.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
