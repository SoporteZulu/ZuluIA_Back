using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

internal class RegistrarDevolucionVentaInternaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoVentasService circuitoVentas)
    : IRequestHandler<RegistrarDevolucionVentaInternaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarDevolucionVentaInternaCommand request, CancellationToken ct)
    {
        var createHandler = new CrearBorradorVentaCommandHandler(comprobanteRepo, db, uow, currentUser);
        var createResult = await createHandler.Handle(
            new CrearBorradorVentaCommand(
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
                request.ComprobanteOrigenId,
                request.Items),
            ct);

        if (!createResult.IsSuccess)
            return createResult;

        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(createResult.Value, ct);
        if (comprobante is null)
            return Result.Failure<long>("No se pudo recuperar la devolución creada.");

        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {comprobante.TipoComprobanteId}.");

        comprobante.Emitir(currentUser.UserId);
        comprobanteRepo.Update(comprobante);

        await circuitoVentas.AplicarEfectosAsync(
            comprobante,
            tipo,
            request.OperacionStock,
            request.OperacionCuentaCorriente,
            ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success(comprobante.Id);
    }
}
