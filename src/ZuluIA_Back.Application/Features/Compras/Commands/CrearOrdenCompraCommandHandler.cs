using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearOrdenCompraCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IRepository<OrdenCompraMeta> ordenCompraRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    TerceroOperacionValidationService terceroOperacionValidationService)
    : IRequestHandler<CrearOrdenCompraCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearOrdenCompraCommand request, CancellationToken ct)
    {
        var createHandler = new CrearBorradorCompraCommandHandler(comprobanteRepo, db, uow, currentUser, terceroOperacionValidationService);
        var createResult = await createHandler.Handle(
            new CrearBorradorCompraCommand(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.Fecha,
                request.FechaVencimiento,
                request.TerceroId,
                request.MonedaId,
                request.Cotizacion,
                request.Percepciones,
                request.Observacion,
                null,
                request.Items),
            ct);

        if (!createResult.IsSuccess)
            return createResult;

        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(createResult.Value, ct);
        if (comprobante is null)
            return Result.Failure<long>("No se pudo recuperar la orden de compra creada.");

        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {comprobante.TipoComprobanteId}.");

        comprobante.Emitir(currentUser.UserId);
        comprobanteRepo.Update(comprobante);

        var cantidadTotal = comprobante.Items.Sum(x => x.Cantidad - x.CantidadBonificada);
        var meta = OrdenCompraMeta.Crear(
            comprobante.Id,
            request.TerceroId,
            request.FechaEntregaReq,
            request.CondicionesEntrega,
            cantidadTotal);

        await ordenCompraRepo.AddAsync(meta, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(meta.Id);
    }
}
