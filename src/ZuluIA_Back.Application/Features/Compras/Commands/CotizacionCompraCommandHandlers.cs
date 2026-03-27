using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearCotizacionCompraCommandHandler(
    ICotizacionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CrearCotizacionCompraCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearCotizacionCompraCommand request, CancellationToken ct)
    {
        if (!request.Items.Any())
            return Result.Failure<long>("La cotización debe tener al menos un ítem.");

        var cotizacion = CotizacionCompra.Crear(
            request.SucursalId, request.RequisicionId,
            request.ProveedorId, request.Fecha,
            request.FechaVencimiento, request.Observacion,
            currentUser.UserId);

        foreach (var item in request.Items)
            cotizacion.AgregarItem(CotizacionCompraItem.Crear(
                0, item.ItemId, item.Descripcion, item.Cantidad, item.PrecioUnitario));

        await repo.AddAsync(cotizacion, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cotizacion.Id);
    }
}

public class AceptarCotizacionCompraCommandHandler(
    ICotizacionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AceptarCotizacionCompraCommand, Result>
{
    public async Task<Result> Handle(AceptarCotizacionCompraCommand request, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(request.Id, ct);
        if (c is null) return Result.Failure("Cotización no encontrada.");
        c.Aceptar(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class RechazarCotizacionCompraCommandHandler(
    ICotizacionCompraRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<RechazarCotizacionCompraCommand, Result>
{
    public async Task<Result> Handle(RechazarCotizacionCompraCommand request, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(request.Id, ct);
        if (c is null) return Result.Failure("Cotización no encontrada.");
        c.Rechazar(currentUser.UserId);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
