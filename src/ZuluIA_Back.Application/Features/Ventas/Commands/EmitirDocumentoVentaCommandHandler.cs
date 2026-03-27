using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class EmitirDocumentoVentaCommandHandler(
    IComprobanteRepository comprobanteRepo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    CircuitoVentasService circuitoVentas)
    : IRequestHandler<EmitirDocumentoVentaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(EmitirDocumentoVentaCommand request, CancellationToken ct)
    {
        var comprobante = await comprobanteRepo.GetByIdConItemsAsync(request.ComprobanteId, ct);
        if (comprobante is null)
            return Result.Failure<long>($"No se encontró el comprobante ID {request.ComprobanteId}.");

        if (comprobante.Estado != EstadoComprobante.Borrador)
            return Result.Failure<long>($"Solo se pueden emitir documentos en borrador. Estado actual: {comprobante.Estado}.");

        var tipo = await db.TiposComprobante
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comprobante.TipoComprobanteId, ct);

        if (tipo is null)
            return Result.Failure<long>($"No se encontró el tipo de comprobante ID {comprobante.TipoComprobanteId}.");

        if (!tipo.EsVenta)
            return Result.Failure<long>("El comprobante no pertenece al circuito de ventas.");

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
